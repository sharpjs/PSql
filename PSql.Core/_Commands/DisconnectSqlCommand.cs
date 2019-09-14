using System.Data.SqlClient;
using System.Management.Automation;

namespace PSql
{
    [Cmdlet(VerbsCommunications.Disconnect, "Sql")]
    [OutputType(typeof(void))]
    public class DisconnectSqlCommand : Cmdlet
    {
        // -Connection
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
        public SqlConnection[] Connection { get; set; }

        protected override void ProcessRecord()
        {
            var connections = Connection;
            if (connections == null)
                return;

            foreach (var connection in connections)
            {
                if (connection == null)
                    continue;

                ConnectionInfo.Get(connection).IsDisconnecting = true;
                connection.Dispose();
            }
        }
    }
}
