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
        CmdletExtensions.WriteHost(
            this, text, newLine, foregroundColor, backgroundColor
        );
    }
}
