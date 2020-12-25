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
    public class SqlConnection : IDisposable
    {
        private readonly dynamic _connection; // M.D.S.SqlConnection

        /// <summary>
        ///   Creates a new <see cref="SqlConnection"/> instance for the
        ///   specified context and database name, logging server messages
        ///   via the specified cmdlet.
        /// </summary>
        /// <param name="context">
        ///   An object containing information necessary to connect to a
        ///   database.  If not provided, the constructor will use a context
        ///   with default property values.
        /// </param>
        /// <param name="databaseName">
        ///   The name of the database to which to connect.  If not provided,
        ///   the constructor connects to the default database for the context.
        /// </param>
        /// <param name="cmdlet">
        ///   The cmdlet whose
        ///     <see cref="Cmdlet.WriteHost(string, bool, ConsoleColor?, ConsoleColor?)"/>
        ///   and
        ///     <see cref="System.Management.Automation.Cmdlet.WriteWarning(string)"/>
        ///   methods will be used to print messges received from the server.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="cmdlet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.Data.Common.DbException">
        ///   A connection-level error occurred while opening the connection.
        /// </exception>
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
        ///   Gets a value indicating whether errors have been logged on the
        ///   connection.
        /// </summary>
        public bool HasErrors
            => PSqlClient.Instance.HasErrors(_connection);

        /// <summary>
        ///   Sets <see cref="HasErrors"/> to <c>false</c>, forgetting about
        ///   any errors prevously logged on the connection.
        /// </summary>
        internal void ClearErrors()
        {
            PSqlClient.Instance.ClearErrors(_connection);
        }

        /// <summary>
        ///   Creates a new <see cref="SqlCommand"/> instance that can execute
        ///   commands on the connection.
        /// </summary>
        internal SqlCommand CreateCommand()
        {
            return new SqlCommand(_connection);
        }

        /// <summary>
        ///   Closes the connection and frees resources owned by it.
        /// </summary>
        public void Dispose()
        {
            Dispose(managed: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Closes the connection and frees resources owned by it.
        /// </summary>
        /// <param name="managed">
        ///   Whether to dispose managed resources.  Unmanaged are always
        ///   disposed.
        /// </param>
        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                // Indicate that disconnection is expected
                PSqlClient.Instance.SetDisconnecting(_connection);

                // Disconect
                _connection.Dispose();
            }
        }
    }
}
