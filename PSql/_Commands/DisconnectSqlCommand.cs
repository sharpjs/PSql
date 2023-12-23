// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql;

[Cmdlet(VerbsCommunications.Disconnect, "Sql")]
[OutputType(typeof(void))]
public class DisconnectSqlCommand : PSCmdlet
{
    // -Connection
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
    public ISqlConnection?[]? Connection { get; set; }

    protected override void ProcessRecord()
    {
        if (Connection is { } connections)
            foreach (var connection in connections)
                connection?.Dispose();
    }
}
