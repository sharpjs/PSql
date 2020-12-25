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
                =  error.Procedure.NullIfEmpty_()
                ?? NonProcedureLocationName;

            return $"{procedure}:{error.LineNumber}: E{error.Class}: {error.Message}";
        }
    }
}
