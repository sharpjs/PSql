// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

/// <summary>
///   Base class for PSql cmdlets that use an open database connection.
/// </summary>
public abstract class ConnectedCmdlet : PSqlCmdlet, IDisposable
{
    protected const string
        ConnectionName = nameof(Connection),
        ContextName    = nameof(Context);

    /// <summary>
    ///   <b>-Connection:</b>
    ///   An open database connection.  Obtain via <c>Connect-Sql</c>.
    /// </summary>
    [Parameter(ParameterSetName = ConnectionName, Mandatory = true)]
    public SqlConnection? Connection { get; set; }

    /// <summary>
    ///   <b>-Context:</b> TODO
    ///   An object containing information necessary to connect to SQL Server,
    ///   Azure SQL Database, or compatible database.  Obtain via
    ///   <c>New-SqlContext</c>.
    /// </summary>
    [Parameter(ParameterSetName = ContextName)]
    [ValidateNotNull]
    public SqlContext? Context { get; set; }

    /// <summary>
    ///   <b>-DatabaseName:</b>
    ///   An optional database name.  If specified, this parameter overrides
    ///   the database name, if any, specified in the <c>-Context</c>.
    /// </summary>
    [Alias("Database")]
    [Parameter(ParameterSetName = ContextName)]
    public string? DatabaseName { get; set; }

    // Whether this object is responsible for disposing its connection
    private bool _ownsConnection;

    protected override void BeginProcessing()
    {
        // NOTE: If any of the parameters above are made to take their values
        // from the pipeline, then this implicit connection setup must move out
        // of BeginProcessing() and into something that runs once for each call
        // to ProcessRecord().

        if (Connection is null)
        {
            Context       ??= new();
            Connection      = Context.Connect(DatabaseName, this);
            _ownsConnection = true;
        }
        else
        {
            _ownsConnection = false;
        }
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        // No unmanaged resources to dispose; use abbreviated Dispose pattern

        if (_ownsConnection)
            Connection?.Dispose();

        Connection      = null;
        _ownsConnection = false;
    }
}
