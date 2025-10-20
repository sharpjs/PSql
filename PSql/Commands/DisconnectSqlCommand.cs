// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql.Commands;

/// <summary>
///   The <c>Disconnect-Sql</c> command.
/// </summary>
[Cmdlet(VerbsCommunications.Disconnect, "Sql")]
[OutputType(typeof(void))]
public class DisconnectSqlCommand : PSqlCmdlet
{
    /// <summary>
    ///   <b>-Connection:</b> TODO
    /// </summary>
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
    public SqlConnection[]? Connection { get; set; }

    protected override void ProcessRecord()
    {
        if (Connection is { } connections)
            foreach (var connection in connections)
                connection?.Dispose();
    }
}
