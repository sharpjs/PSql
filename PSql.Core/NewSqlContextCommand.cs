using System;
using System.Management.Automation;
using static System.Data.SqlClient.ApplicationIntent;

namespace PSql
{
    [Cmdlet(VerbsCommon.New, nameof(SqlContext), DefaultParameterSetName = GenericName)]
    [OutputType(typeof(SqlContext))]
    public class NewSqlContextCommand : Cmdlet
    {
        private const string
            GenericName = "Generic",
            AzureName   = "Azure";

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
        [Alias("s", "sn", "Server")]
        [Parameter(ParameterSetName = GenericName, Position = 0,                   ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        // -DatabaseName
        [Alias("d", "dn", "Database")]
        [Parameter(ParameterSetName = GenericName, Position = 1, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 3, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        // -Credential
        [Alias("c")]
        [Parameter(ParameterSetName = GenericName, Position = 2,                    ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 4, Mandatory = true,  ValueFromPipelineByPropertyName = true)]
        [Credential]
        public PSCredential Credential { get; set; } = PSCredential.Empty;

        // -EncryptionMode
        [Alias("e", "em", "Encryption")]
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        public EncryptionMode EncryptionMode { get; set; }

        // -ReadOnlyIntent
        [Alias("ro", "ReadOnly")]
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

        // -ConnectTimeout
        [Alias("t", "to", "Timeout")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateRange("0:00:00", "24855.03:14:07")]
        public TimeSpan? ConnectTimeout { get; set; }

        protected override void ProcessRecord()
        {
            var context = Azure.IsPresent
                ? new AzureSqlContext { ResourceGroupName = ResourceGroupName }
                : new SqlContext();

            var credential = Credential.IsNullOrEmpty()
                ? null
                : Credential;

            context.ServerName        = ServerName;
            context.DatabaseName      = DatabaseName;
            context.Credential        = credential;
            context.EncryptionMode    = Azure ? EncryptionMode.Full : EncryptionMode;
            context.ConnectTimeout    = ConnectTimeout;
            context.ClientName        = ClientName;
            context.ApplicationName   = ApplicationName;
            context.ApplicationIntent = ReadOnlyIntent ? ReadOnly : ReadWrite;

            WriteObject(context);
        }
    }
}
