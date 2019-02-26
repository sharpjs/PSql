using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PSql
{
    /// <summary>
    ///   Metadata maintained by PSql about each database connection.
    /// </summary>
    internal class ConnectionInfo
    {
        private static readonly Dictionary<SqlConnection, ConnectionInfo>
            Entries = new Dictionary<SqlConnection, ConnectionInfo>();

        private ConnectionInfo() { }

        /// <summary>
        ///   Gets whether the connection is disconnecting.
        /// </summary>
        public bool IsDisconnecting { get; internal set; }

        /// <summary>
        ///   Gets whether the connection has produced error messages.
        /// </summary>
        public bool HasErrors { get; internal set; }

        /// <summary>
        ///   Gets or creates PSql metadata for the given connection.
        /// </summary>
        /// <param name="connection">
        ///   The connection for which to get or create metadata.
        /// </param>
        /// <returns>
        ///   The metadata for <paramref name="connection"/>.
        /// </returns>
        public static ConnectionInfo Get(SqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            lock (Entries)
            {
                // Find in tracker
                if (Entries.TryGetValue(connection, out var entry))
                    return entry;

                // Register in tracker
                entry = new ConnectionInfo();
                Entries.Add(connection, entry);

                // Unregister on disposal
                connection.Disposed += HandleConnectionDisposed;

                return entry;
            }
        }

        private static void HandleConnectionDisposed(object sender, EventArgs e)
        {
            var connection = (SqlConnection) sender;

            lock (Entries)
            {
                // Find in tracker
                if (!Entries.TryGetValue(connection, out var entry))
                    return;

                // Unregister from tracker
                Entries.Remove(connection);

                // Detect unexpected close
                if (entry.IsDisconnecting)
                    return; // expected

                // Present unexpected close
                throw new DataException(
                    "The connection to the database server was closed unexpectedly."
                );
            }
        }
    }
}
