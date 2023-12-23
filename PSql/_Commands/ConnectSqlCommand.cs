// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql;

[Cmdlet(VerbsCommunications.Connect, "Sql")]
[OutputType(typeof(ISqlConnection))]
public class ConnectSqlCommand : PSCmdlet
{
    // -Context
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public SqlContext? Context { get; set; }

    // -DatabaseName
    [Parameter]
    [Alias("Database")]
    public string? DatabaseName { get; set; }

    protected override void ProcessRecord()
    {
        var context    = Context ?? new();
        var connection = context.Connect(DatabaseName, this);

        WriteObject(connection);
    }
}
