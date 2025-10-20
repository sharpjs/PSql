// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

/// <summary>
///   Base class for PSql cmdlets.
/// </summary>
public class PSqlCmdlet : PSCmdlet, ICmdlet
{
    /// <inheritdoc/>
    public void WriteHost(
        string?       text,
        bool          newLine         = true,
        ConsoleColor? foregroundColor = null,
        ConsoleColor? backgroundColor = null)
    {
        // Technique learned from PSv5+ Write-Host implementation, which works
        // by sending specially-marked messages to the information stream.
        //
        // https://github.com/PowerShell/PowerShell/blob/v7.5.4/src/Microsoft.PowerShell.Commands.Utility/commands/utility/WriteConsoleCmdlet.cs

        var data = new HostInformationMessage
        {
            Message         = text ?? "",
            NoNewLine       = !newLine,
            ForegroundColor = foregroundColor,
            BackgroundColor = backgroundColor,
        };

        WriteInformation(data, HostTag);
    }

    private static readonly string[] HostTag = ["PSHOST"];
}
