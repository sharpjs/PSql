// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

[Cmdlet(VerbsCommunications.Connect, "Sql")]
[OutputType(typeof(SqlConnection))]
public class ConnectSqlCommand : Cmdlet
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
        var connection = (Context ?? new()).CreateConnection(DatabaseName, this);

        WriteObject(connection);
    }
}
