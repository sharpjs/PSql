// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql.Commands;

/// <summary>
///   The <c>Connect-Sql</c> command.
/// </summary>
[Cmdlet(VerbsCommunications.Connect, "Sql")]
[OutputType(typeof(SqlConnection))]
public class ConnectSqlCommand : PSqlCmdlet
{
    /// <summary>
    ///   <b>-Context:</b> TODO
    /// </summary>
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public SqlContext? Context { get; set; }

    /// <summary>
    ///   <b>-DatabaseName:</b> TODO
    /// </summary>
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
