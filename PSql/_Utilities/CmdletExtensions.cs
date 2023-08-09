// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Management.Automation.Host;

namespace PSql;

/// <summary>
///   Extension methods for <see cref="Cmdlet"/> and <see cref="PSCmdlet"/>.
/// </summary>
public static class CmdletExtensions
{
    private static readonly string[] HostTag = { "PSHOST" };

    /// <summary>
    ///   Writes the specified message as a host information message.
    /// </summary>
    /// <param name="cmdlet">
    ///   The cmdlet that should write the message.
    /// </param>
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
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   This method is similar to the PowerShell <c>Write-Host</c> cmdlet.
    /// </remarks>
    public static void WriteHost(
        this Cmdlet   cmdlet,
        string?       message,
        bool          newLine         = true,
        ConsoleColor? foregroundColor = null,
        ConsoleColor? backgroundColor = null)
    {
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

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

        cmdlet.WriteInformation(data, HostTag);
    }
}
