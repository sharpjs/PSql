// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   An abstract view of a PSql PowerShell cmdlet.
/// </summary>
public interface ICmdlet
{
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
        ConsoleColor? backgroundColor = null);

    /// <summary>
    ///   Writes the specified warning message to the host.
    /// </summary>
    /// <remarks>
    ///   This method is similar to the PowerShell <c>Write-Warning</c> cmdlet.
    /// </remarks>
    void WriteWarning(string message);
}
