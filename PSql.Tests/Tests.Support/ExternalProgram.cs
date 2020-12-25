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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PSql.Tests
{
    public class ExternalProgram
    {
        protected ProcessStartInfo Info { get; }

        public ExternalProgram(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("Argument must not be empty.", nameof(name));

            Info = new ProcessStartInfo
            {
                FileName               = name,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
            };
        }

        public ExternalProgram WithArguments(params string[] args)
        {
            foreach (var arg in args)
                Info.ArgumentList.Add(arg);

            return this;
        }

        public ExternalProgram WithArguments(IEnumerable<string> args)
        {
            foreach (var arg in args)
                Info.ArgumentList.Add(arg);

            return this;
        }

        public (int ExitCode, string Output) Run()
        {
            using var process = new Process { StartInfo = Info };

            var output = new StringBuilder();
            process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
            process.ErrorDataReceived  += (_, e) => output.AppendLine(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return (process.ExitCode, output.ToString());
        }

        public string Run(int expecting)
        {
            var (exitCode, output) = Run();

            if (exitCode != expecting)
                throw NewExitedWithCodeException(exitCode, output);

            return output;
        }

        public T Run<T>(Func<int, string, T> projection)
        {
            if (projection is null)
                throw new ArgumentNullException(nameof(projection));

            var (exitCode, output) = Run();

            return projection(exitCode, output);
        }

        public Exception NewExitedWithCodeException(int exitCode, string? output = null)
        {
            var message = new StringBuilder()
                .AppendFormat("{0} exited with code {1}.", Info.FileName, exitCode);

            if (output.HasContent())
                message
                    .AppendLine()
                    .AppendLine("----- BEGIN OUTPUT -----")
                    .AppendLine(output)
                    .AppendLine("----- END OUTPUT -----");

            return new ExternalException(message.ToString())
            {
                Data =
                {
                    ["ExitCode"] = exitCode,
                    ["Output"]   = output
                }
            };
        }
    }
}
