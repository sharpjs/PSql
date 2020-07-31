using System.Management.Automation;
using Microsoft.Data.SqlClient;

namespace PSql
{
    [Cmdlet(VerbsCommunications.Connect, "Sql")]
    [OutputType(typeof(SqlConnection))]
    public class ConnectSqlCommand : Cmdlet
    {
        // -Context
        [Parameter(ValueFromPipeline = true)]
        public SqlContext Context { get; set; }

        // -DatabaseName
        [Parameter]
        [Alias("Database")]
        public string DatabaseName { get; set; }

        protected override void ProcessRecord()
        {
            (var connection, _) = EnsureConnection(null, Context, DatabaseName);

            WriteObject(connection);
        }
    }
}
