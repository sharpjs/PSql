/*
    Copyright 2020 Jeffrey Sharp

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

        internal SqlConnection(SqlContext? context, string? databaseName, Cmdlet cmdlet)
        {
            if (cmdlet is null)
                throw new ArgumentNullException(nameof(cmdlet));

            context ??= new SqlContext();

            var client           = PSqlClient.Instance;
            var connectionString = context.GetConnectionString(databaseName);
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
        ///   Gets a value indicating whether or not errors have been logged
        ///   on the connection.
        /// </summary>
        public bool HasErrors
            => PSqlClient.Instance.HasErrors(_connection);

        /// <summary>
        ///   Closes the connection and frees resources owned by it.
        /// </summary>
        public void Dispose()
        {
            Dispose(managed: true);
            GC.SuppressFinalize(this);
        }

        internal void ClearErrors()
        {
            // Clear any failures from prior command
            PSqlClient.Instance.ClearErrors(_connection);
        }

        internal SqlCommand CreateCommand(Cmdlet cmdlet)
        {
            return new SqlCommand(_connection, cmdlet);
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
