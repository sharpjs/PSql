// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql;

/// <summary>
///   Base class for PSql cmdlets that use an open database connection.
/// </summary>
public abstract class ConnectedCmdlet : PSCmdlet, IDisposable
{
    protected const string
        ConnectionName = nameof(Connection),
        ContextName    = nameof(Context);

    // -Connection
    [Parameter(ParameterSetName = ConnectionName, Mandatory = true)]
    public ISqlConnection? Connection { get; set; }

    // -Context
    [Parameter(ParameterSetName = ContextName)]
    [ValidateNotNull]
    public SqlContext? Context { get; set; }

    // -DatabaseName
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

    ~ConnectedCmdlet()
    {
        Dispose(managed: false);
    }

    void IDisposable.Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        if (_ownsConnection)
            Connection?.Dispose();

        Connection      = null;
        _ownsConnection = false;
    }
}
