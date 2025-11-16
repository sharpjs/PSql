// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

/// <summary>
///   The <c>Connect-Sql</c> command.
///   Opens a connection to SQL Server, Azure SQL Database, or compatible
///   database.
/// </summary>
[Cmdlet(VerbsCommunications.Connect, "Sql")]
[OutputType(typeof(SqlConnection))]
public class ConnectSqlCommand : PSqlCmdlet
{
    /// <summary>
    ///   <b>-Context:</b>
    ///   An object containing information necessary to connect to SQL Server,
    ///   Azure SQL Database, or compatible database.  Obtain via
    ///   <c>New-SqlContext</c>.
    /// </summary>
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public SqlContext? Context { get; set; }

    /// <summary>
    ///   <b>-DatabaseName:</b>
    ///   An optional database name.  If specified, this parameter overrides
    ///   the database name, if any, specified in the <c>-Context</c>.
    /// </summary>
    [Parameter]
    [Alias("Database")]
    public string? DatabaseName { get; set; }

    /// <inheritdoc/>
    protected override void ProcessRecord()
    {
        var context    = Context ?? new();
        var connection = context.Connect(DatabaseName, this);

        WriteObject(connection);
    }
}
