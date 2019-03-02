using System.Data.SqlClient;
using System.Management.Automation;

namespace PSql
{
    [Cmdlet(VerbsCommunications.Disconnect, "Sql")]
    public class DisconnectSqlCommand : Cmdlet
    {
        // -Connection
        [Alias("c", "cn")]
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
        public SqlConnection[] Connection { get; set; }

        protected override void ProcessRecord()
        {
            foreach (var connection in Connection)
            {
                if (connection == null)
                    continue;

                ConnectionInfo.Get(connection).IsDisconnecting = true;
                connection.Dispose();
            }
        }
    }
}
