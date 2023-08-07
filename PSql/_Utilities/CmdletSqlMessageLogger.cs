// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   An <see cref="ISqlMessageLogger"/> implementation that logs server
///   messages using the <c>Write…</c> methods of a PowerShell cmdlet.
/// </summary>
public class CmdletSqlMessageLogger : ISqlMessageLogger
{
    /// <summary>
    ///   Initializes a new <see cref="CmdletSqlMessageLogger"/> instance that
    ///   logs server messages using the specified PowerShell cmdlet.
    /// </summary>
    /// <param name="cmdlet">
    ///   The PowerShell cmdlet to use to log server messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    public CmdletSqlMessageLogger(Cmdlet cmdlet)
    {
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        Cmdlet = cmdlet;
    }

    /// <summary>
    ///   Gets the PowerShell cmdlet to use to log server messages.
    /// </summary>
    public Cmdlet Cmdlet { get; }

    /// <inheritdoc/>
    public void LogInformation(string message)
    {
        // Use Write-Host equivalent since 

        Cmdlet.WriteHost(message);
    }

    /// <inheritdoc/>
    public void LogError(string message)
    {
        // Log server messages of error severity (11 and higher) as warnings
        // initially, then 

        Cmdlet.WriteWarning(message);
    }
}
