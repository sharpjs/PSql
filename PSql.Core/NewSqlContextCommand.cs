using System.Management.Automation;
using static System.Data.SqlClient.ApplicationIntent;

namespace PSql
{
    [Cmdlet(VerbsCommon.New, nameof(SqlContext), DefaultParameterSetName = GenericName)]
    [OutputType(typeof(SqlContext))]
    public class NewSqlContextCommand : Cmdlet
    {
        private const string
            GenericName        = "Generic",
            AzureName          = "Azure",
            LocalServerName    = ".",
            MasterDatabaseName = "master";

        // -Azure
        [Alias("a")]
        [Parameter(ParameterSetName = AzureName, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Azure { get; set; }

        // -ResourceGroupName
        [Alias("g", "rg", "ResourceGroup")]
        [Parameter(ParameterSetName = AzureName, Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        // -ServerName
        [Alias("s", "Server")]
        [Parameter(ParameterSetName = GenericName, Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 2, Mandatory = true,  ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; } = LocalServerName;

        // -DatabaseName
        [Alias("d", "Database")]
        [Parameter(ParameterSetName = GenericName, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 3, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; } = MasterDatabaseName;

        // -Credential
        [Alias("c")]
        [Parameter(ParameterSetName = GenericName, Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 4, Mandatory = true,  ValueFromPipelineByPropertyName = true)]
        [Credential]
        public PSCredential Credential { get; set; } = PSCredential.Empty;

        // -EncryptionMode
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        public EncryptionMode EncryptionMode { get; set; }

        // -ReadOnlyIntent
        [Alias("ro")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter ReadOnlyIntent { get; set; }

        // -ClientName
        [Alias("cn", "Client")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ClientName { get; set; }

        // -ApplicationName
        [Alias("an", "Application")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ApplicationName { get; set; }

        // -ConnectTimeoutSeconds
        [Alias("to", "ConnectTimeout")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateRange(0, int.MaxValue)]
        public int? ConnectTimeoutSeconds { get; set; }

        protected override void ProcessRecord()
        {
            var context = Azure.IsPresent
                ? new AzureSqlContext { ResourceGroupName = ResourceGroupName }
                : new SqlContext();

            var credential = Credential == null || Credential == PSCredential.Empty
                ? null
                : Credential;

            context.ServerName               = ServerName;
            context.DatabaseName             = DatabaseName;
            context.Credential               = credential;
            context.EncryptionMode           = Azure ? EncryptionMode.Full : EncryptionMode;
            context.ConnectionTimeoutSeconds = ConnectTimeoutSeconds;
            context.ClientName               = ClientName;
            context.ApplicationName          = ApplicationName;
            context.ApplicationIntent        = ReadOnlyIntent ? ReadOnly : ReadWrite;

            WriteObject(context);
        }
    }
}
