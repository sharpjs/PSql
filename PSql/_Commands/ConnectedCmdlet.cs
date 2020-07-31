using System;
using System.Management.Automation;
using Microsoft.Data.SqlClient;

namespace PSql
{
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
        public SqlConnection Connection { get; set; }

        // -Context
        [Parameter(ParameterSetName = ContextName)]
        [ValidateNotNull]
        public SqlContext Context { get; set; }

        // -DatabaseName
        [Alias("Database")]
        [Parameter(ParameterSetName = ContextName)]
        public string DatabaseName { get; set; }

        private bool _ownsConnection;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            (Connection, _ownsConnection) = EnsureConnection(Connection, Context, DatabaseName);
        }

        protected virtual void Dispose(bool managed)
        {
            if (managed && _ownsConnection)
            {
                // Indicate that disconnection is expected
                ConnectionInfo.Get(Connection).IsDisconnecting = true;

                // Disconnect
                Connection.Dispose();
                Connection = null;
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
    }
}
