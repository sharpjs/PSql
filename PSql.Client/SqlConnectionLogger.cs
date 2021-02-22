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
using Microsoft.Data.SqlClient;

namespace PSql
{
    internal class SqlConnectionLogger
    {
        public static void Use(
            SqlConnection  connection,
            Action<string> writeInformation,
            Action<string> writeWarning)
        {
            new SqlConnectionLogger(writeInformation, writeWarning).Attach(connection);
        }

        public SqlConnectionLogger(
            Action<string> writeInformation,
            Action<string> writeWarning)
        {
            WriteInformation = writeInformation
                ?? throw new ArgumentNullException(nameof(writeInformation));

            WriteWarning = writeWarning
                ?? throw new ArgumentNullException(nameof(writeWarning));
        }

        public Action<string> WriteInformation { get; }
        public Action<string> WriteWarning     { get; }

        public void Attach(SqlConnection connection)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            connection.FireInfoMessageEventOnUserErrors  = true;
            connection.InfoMessage                      += HandleMessage;
        }

        private void HandleMessage(object sender, SqlInfoMessageEventArgs e)
        {
            const int MaxInformationalSeverity = 10;

            if (sender is not SqlConnection connection)
                return;

            foreach (SqlError? error in e.Errors)
            {
                if (error is null)
                {
                    // Do nothing
                }
                else if (error.Class <= MaxInformationalSeverity)
                {
                    // Output as normal text
                    WriteInformation(error.Message);
                }
                else
                {
                    // Output as warning
                    WriteWarning(Format(error));

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
