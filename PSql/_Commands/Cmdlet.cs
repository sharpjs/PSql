/*
    Copyright 2021 Jeffrey Sharp

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
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PSql
{
    /// <summary>
    ///   Base class for PSql cmdlets.
    /// </summary>
    public abstract class Cmdlet : System.Management.Automation.Cmdlet
    {
        private static readonly string[]
            HostTag = { "PSHOST" };

        public void WriteHost(string message,
            bool          newLine         = true,
            ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null)
        {
            // Technique learned from PSv5+ Write-Host implementation, which
            // works by sending specially-marked messages to the information
            // stream.
            //
            // https://github.com/PowerShell/PowerShell/blob/v7.0.3/src/Microsoft.PowerShell.Commands.Utility/commands/utility/WriteConsoleCmdlet.cs

            var data = new HostInformationMessage
            {
                Message   = message,
                NoNewLine = !newLine
            };

            if (foregroundColor.HasValue || backgroundColor.HasValue)
            {
                try
                {
                    data.ForegroundColor = foregroundColor;
                    data.BackgroundColor = backgroundColor;
                }
                catch (HostException)
                {
                    // Host is non-interactive or does not support colors.
                }
            }

            WriteInformation(data, HostTag);
        }
    }
}
