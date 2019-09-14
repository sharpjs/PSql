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
            (var connection, _) = EnsureConnection(null, Context, DatabaseName);

            WriteObject(connection);
        }
    }
}
