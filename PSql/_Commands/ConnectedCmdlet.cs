/*
    Copyright 2021 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Management.Automation;

namespace PSql;

using static SqlConnectionHelper;

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
        (Connection, _ownsConnection)
            = EnsureConnection(Connection, Context, DatabaseName, this);
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
