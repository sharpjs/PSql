using System.Management.Automation;
using Microsoft.Data.SqlClient;

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

                // Indicate that disconnection is expected
                ConnectionInfo.Get(connection).IsDisconnecting = true;

                connection.Dispose();
            }
        }
    }
}
