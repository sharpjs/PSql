using System;
using System.Data;

namespace PSql
{
    /// <summary>
    ///   Represents a connection to SQL Server, Azure SQL Database, or
    ///   compatible product.
    /// </summary>
    /// <remarks>
    ///   This type is a proxy for
    ///   <c>Microsoft.Data.SqlClient.SqlConnection.</c>
    /// </remarks>
    public class SqlConnection : IDisposable // ~> M.D.S.SqlConnection
    {
        private readonly dynamic _connection;

        internal SqlConnection(Cmdlet cmdlet, SqlContext context)
        {
            if (cmdlet is null)
                throw new ArgumentNullException(nameof(cmdlet));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var client           = PSqlClient.Instance;
            var connectionString = context.GetConnectionString();
            var credential       = context.Credential;
            var writeInformation = new Action<string>(s => cmdlet.WriteHost   (s));
            var writeWarning     = new Action<string>(s => cmdlet.WriteWarning(s));

            _connection = credential.IsNullOrEmpty()
                ? client.Connect(
                    connectionString,
                    writeInformation,
                    writeWarning
                )
                : client.Connect(
                    connectionString,
                    credential!.UserName,
                    credential!.Password,
                    writeInformation,
                    writeWarning
                );
        }

        /// <summary>
        ///   Gets the connection string used to create this connection.  The
        ///   connection string includes server name, database name, and other
        ///   parameters that control the initial opening of the connection.
        /// </summary>
        public string ConnectionString
            => _connection.ConnectionString;

        /// <summary>
        ///   Gets a value indicating whether the connection is open.  The
        ///   value is <c>true</c> (open) for new connections and transitions
        ///   permanently to <c>false</c> (closed) when the connection closes.
        /// </summary>
        public bool IsOpen
            => (int) _connection.State == (int) ConnectionState.Open;

        /// <summary>
        ///   Closes the connection and frees resources owned by it.
        /// </summary>
        public void Dispose()
        {
            Dispose(managed: true);
            GC.SuppressFinalize(this);
        }

        private protected virtual void Dispose(bool managed)
        {
            if (!managed)
                return;

            // Indicate that disconnection is expected
            PSqlClient.Instance.SetDisconnecting(_connection);

            _connection.Dispose();
        }
    }
}
