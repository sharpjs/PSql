// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Management.Automation.Host;

namespace PSql;

/// <summary>
///   Base class for PSql cmdlets.
/// </summary>
public abstract class Cmdlet : System.Management.Automation.Cmdlet
{
    private static readonly string[] HostTag = { "PSHOST" };

    /// <summary>
    ///   Writes the specified message to the host.
    /// </summary>
    /// <param name="message">
    ///   The message to write.
    /// </param>
    /// <param name="newLine">
    ///   Whether a newline should follow the message.
    /// </param>
    /// <param name="foregroundColor">
    ///   The foreground color to use.
    /// </param>
    /// <param name="backgroundColor">
    ///   The background color to use.
    /// </param>
    /// <remarks>
    ///   This method is similar to the PowerShell <c>Write-Host</c> cmdlet.
    /// </remarks>
    public void WriteHost(
        string        message,
        bool          newLine         = true,
        ConsoleColor? foregroundColor = null,
        ConsoleColor? backgroundColor = null)
    {
        // Technique learned from PSv5+ Write-Host implementation, which works
        // by sending specially-marked messages to the information stream.
        //
        // https://github.com/PowerShell/PowerShell/blob/v7.0.3/src/Microsoft.PowerShell.Commands.Utility/commands/utility/WriteConsoleCmdlet.cs

        var data = new HostInformationMessage
        {
            Message   = message ?? "",
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
