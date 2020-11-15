/*
    Copyright 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using FluentAssertions.Extensions;
using Newtonsoft.Json;

namespace PSql.Tests
{
    using static SecureStringHelpers;

    internal class SqlServer : IDisposable
    {
        const int
            StartWaitTime = 60; // seconds

        const string
            Collation     = "Latin1_General_100_CI_AI_SC_UTF8",
            MemoryLimitMb = "2048";

        public SqlServer()
        {
            Credential = new NetworkCredential("sa", GeneratePassword());
            Id         = StartContainer();

            try
            {
                WaitForContainerReady();
                Port = GetContainerHostPort();
            }
            catch
            {
                StopContainer();
                throw;
            }
        }

        public virtual void Dispose()
        {
            StopContainer();
        }

        public string Id { get; }

        public ushort Port { get; }

        public NetworkCredential Credential { get; }

        private string StartContainer()
        {
            var id = Run(
                "docker", "run", "-d", "--rm",
                "-P",
                "-e", "ACCEPT_EULA="           + "Y",
                "-e", "MSSQL_SA_PASSWORD="     + Credential.Password,
                "-e", "MSSQL_COLLATION="       + Collation,
                "-e", "MSSQL_MEMORY_LIMIT_MB=" + MemoryLimitMb,
                "mcr.microsoft.com/mssql/server:2019-latest"
            );

            id = id.TrimEnd();
            id.Should().NotBeEmpty("docker run should output a container id");

            return id;
        }

        private void WaitForContainerReady()
        {
            var deadline = DateTime.UtcNow + StartWaitTime.Seconds();

            for(;;)
            {
                var (status, output) = TryRun(
                    "docker", "exec", Id,
                    "/opt/mssql-tools/bin/sqlcmd",
                    "-S", ".",
                    "-U", "sa",
                    "-P", Credential.Password,
                    "-Q", "PRINT 'OK';"
                );

                if (status == 0)
                    return;

                if (DateTime.UtcNow < deadline)
                    continue;

                throw new TimeoutException(
                    "The SQL Server container did not become ready within the expected time."
                );
            }
        }

        private ushort GetContainerHostPort()
        {
            var json = Run("docker", "inspect", Id);
            var info = (dynamic) JsonConvert.DeserializeObject(json)!;
            var port = (ushort) info[0].NetworkSettings.Ports["1433/tcp"][0].HostPort;

            port.Should().BeInRange(1, 65535,
                "docker inspect should show container port 1433/tcp mapped to a valid host port"
            );

            return port;
        }

        private void StopContainer()
        {
            Run("docker", "kill", Id);
        }

        internal static string Run(string program, params string[] args)
        {
            var (exitCode, output) = TryRun(program, args);

            if (exitCode == 0)
                return output;

            Console.Error.Write(output);

            throw new ExternalException($"{program} exited with code {exitCode}.");
        }

        internal static (int, string) TryRun(string program, params string[] args)
        {
            var info = new ProcessStartInfo
            {
                FileName               = program,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };

            foreach (var arg in args)
                info.ArgumentList.Add(arg);

            int exitCode;
            var output = new StringBuilder();

            using (var process = new Process())
            {
                process.StartInfo           = info;
                process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
                process.ErrorDataReceived  += (_, e) => output.AppendLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            return (exitCode, output.ToString());
        }
    }
}
