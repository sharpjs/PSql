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
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security;
using Microsoft.Data.SqlClient;
using Path = System.IO.Path;

namespace PSql
{
    using static RuntimeInformation;

    /// <summary>
    ///   Top-level interface between PSql and PSql.Client.
    /// </summary>
    public class PSqlClient
    {
        // private Action <string>                   WriteInformation { get; }
        // private Action <string>                   WriteWarning     { get; }
        // private Action <object>                   WriteOutput      { get; }
        // private Func   <object>                   CreateObject     { get; }
        // private Action <object, string, object?>  SetProperty      { get; }

        public PSqlClient()
        {
            SniLoader.Load();
        }

        public SqlConnectionStringBuilder CreateConnectionStringBuilder()
            => new SqlConnectionStringBuilder();

        public SqlConnection Connect(
            string         connectionString,
            Action<string> writeInformation,
            Action<string> writeWarning)
        {
            return ConnectCore(
                new SqlConnection(connectionString),
                writeInformation,
                writeWarning
            );
        }

        public SqlConnection Connect(
            string         connectionString,
            string         username,
            SecureString   password,
            Action<string> writeInformation,
            Action<string> writeWarning)
        {
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            if (!password.IsReadOnly())
                (password = password.Copy()).MakeReadOnly();

            var credential = new SqlCredential(username, password);

            return ConnectCore(
                new SqlConnection(connectionString, credential),
                writeInformation,
                writeWarning
            );
        }

        private SqlConnection ConnectCore(
            SqlConnection  connection,
            Action<string> writeInformation,
            Action<string> writeWarning)
        {
            var info = null as ConnectionInfo;

            try
            {
                info = ConnectionInfo.Get(connection);

                connection.FireInfoMessageEventOnUserErrors = true;
                connection.InfoMessage += (sender, e) =>
                {
                    if (sender is SqlConnection c)
                        HandleMessage(c, e.Errors, writeInformation, writeWarning);
                };

                connection.Open();

                return connection;
            }
            catch
            {
                if (info != null)
                    info.IsDisconnecting = true;

                connection?.Dispose();
                throw;
            }
        }

        private void HandleMessage(
            SqlConnection      connection,
            SqlErrorCollection errors,
            Action<string>     writeInformation,
            Action<string>     writeWarning)
        {
            const int MaxInformationalSeverity = 10;

            foreach (SqlError? error in errors)
            {
                if (error is null)
                {
                    // Do nothing
                }
                else if (error.Class <= MaxInformationalSeverity)
                {
                    // Output as normal text
                    writeInformation(error.Message);
                }
                else
                {
                    // Output as warning
                    writeWarning(Format(error));

                    // Mark current command as failed
                    ConnectionInfo.Get(connection).HasErrors = true;
                }
            }
        }

        private static string Format(SqlError error)
        {
            const string NonProcedureLocationName = "(batch)";

            var procedure
                =  error.Procedure.NullIfEmpty()
                ?? NonProcedureLocationName;

            return $"{procedure}:{error.LineNumber}: E{error.Class}: {error.Message}";
        }
    }
}
