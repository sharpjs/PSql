// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

/// <summary>
///   The <c>Disconnect-Sql</c> command.
///   Closes a connection created by <c>Connect-Sql</c>.
/// </summary>
[Cmdlet(VerbsCommunications.Disconnect, "Sql")]
[OutputType(typeof(void))]
public class DisconnectSqlCommand : PSqlCmdlet
{
    /// <summary>
    ///   <b>-Connection:</b>
    ///   An open database connection.  Obtain via <c>Connect-Sql</c>.
    /// </summary>
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
    public SqlConnection[]? Connection { get; set; }

    /// <inheritdoc/>
    protected override void ProcessRecord()
    {
        if (Connection is { } connections)
            foreach (var connection in connections)
                connection?.Dispose();
    }
}
