// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Base class for PSql cmdlets that use an open database connection.
/// </summary>
public abstract class ConnectedCmdlet : Cmdlet, IDisposable
{
    protected const string
        ConnectionName = nameof(Connection),
        ContextName    = nameof(Context);

    // -Connection
    [Parameter(ParameterSetName = ConnectionName, Mandatory = true)]
    public SqlConnection? Connection { get; set; }

    // -Context
    [Parameter(ParameterSetName = ContextName)]
    [ValidateNotNull]
    public SqlContext? Context { get; set; }

    // -DatabaseName
    [Alias("Database")]
    [Parameter(ParameterSetName = ContextName)]
    public string? DatabaseName { get; set; }

    private bool _ownsConnection;

    protected override void BeginProcessing()
    {
        if (Connection is null)
        {
            Context       ??= new();
            Connection      = Context.CreateConnection(DatabaseName, this);
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
