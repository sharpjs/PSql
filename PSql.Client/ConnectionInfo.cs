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

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

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

        private static void HandleConnectionDisposed(object? sender, EventArgs e)
        {
            if (sender is not SqlConnection connection)
                return;

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
