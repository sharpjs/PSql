using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;

#nullable enable

namespace PSql.Tests.Connected
{
    using static SecureStringHelpers;

    internal class SqlServer : IDisposable
    {
        const string
            Collation     = "Latin1_General_100_CI_AI_SC_UTF8",
            MemoryLimitMb = "2048";

        public SqlServer()
        {
            Credential = new NetworkCredential("sa", GeneratePassword());

            var id = Run(
                "docker", "run", "-d", "--rm",
                "-P",
                "-e", "ACCEPT_EULA="           + "Y",
                "-e", "MSSQL_SA_PASSWORD="     + Credential.Password,
                "-e", "MSSQL_COLLATION="       + Collation,
                "-e", "MSSQL_MEMORY_LIMIT_MB=" + MemoryLimitMb,
                "mcr.microsoft.com/mssql/server:2019-latest"
            );

            Id = id.TrimEnd();
            Id.Should().NotBeEmpty();

            try
            {
                var json = Run("docker", "inspect", Id);
                var info = (dynamic) JsonConvert.DeserializeObject(json)!;

                Port = info[0].NetworkSettings.Ports["1433/tcp"][0].HostPort;
                Port.Should().BeInRange(1, 65535);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public virtual void Dispose()
        {
            Run("docker", "kill", Id);
        }

        public string Id { get; }

        public int Port { get; }

        public NetworkCredential Credential { get; }

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
