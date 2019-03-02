using System.Data.SqlClient;
using System.Management.Automation;

namespace PSql
{
    [Cmdlet(VerbsCommunications.Connect, "Sql", DefaultParameterSetName = BasicName)]
    [OutputType(typeof(SqlConnection))]
    public class ConnectSqlCommand : Cmdlet
    {
        private const string
            BasicName   = "Basic",
            ContextName = "Context";

        // -Context
        [Alias("c")]
        [Parameter(ParameterSetName = ContextName, Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNull]
        public SqlContext Context { get; set; }

        // -ServerName
        [Alias("s", "sn", "Server")]
        [Parameter(ParameterSetName = BasicName, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        // -DatabaseName
        [Alias("d", "dn", "Database")]
        [Parameter(Position = 1)]
        public string DatabaseName { get; set; }

        protected override void ProcessRecord()
        {
            if (Context == null)
            {
                Context = new SqlContext
                {
                    ServerName   = ServerName,
                    DatabaseName = DatabaseName
                };
            }

            var connection = null as SqlConnection;
            var info       = null as ConnectionInfo;

            try
            {
                connection = Context.CreateConnection();
                info       = ConnectionInfo.Get(connection);

                connection.FireInfoMessageEventOnUserErrors = true;
                connection.InfoMessage += HandleConnectionMessage;

                connection.Open();

                WriteObject(connection);
            }
            catch
            {
                if (info != null)
                    info.IsDisconnecting = true;

                connection?.Dispose();
                throw;
            }
        }

        private void HandleConnectionMessage(object sender, SqlInfoMessageEventArgs e)
        {
            const int    MaxInformationalSeverity = 10;
            const string NonProcedureLocationName = "(batch)";

            var connection = (SqlConnection) sender;

            foreach (SqlError error in e.Errors)
            {
                if (error.Class <= MaxInformationalSeverity)
                {
                    WriteHost(error.Message);
                }
                else
                {
                    // Output as warning
                    var procedure = error.Procedure ?? NonProcedureLocationName;
                    var formatted = $"{procedure}:{error.LineNumber}: E{error.Class}: {error.Message}";
                    WriteWarning(formatted);

                    // Mark current command as failed
                    ConnectionInfo.Get(connection).HasErrors = true;
                }
            }
        }
    }
}
