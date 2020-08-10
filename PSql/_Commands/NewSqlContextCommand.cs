using System;
using System.Management.Automation;
using static Microsoft.Data.SqlClient.ApplicationIntent;

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
        [Parameter(ParameterSetName = AzureName, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Azure { get; set; }

        // -ResourceGroupName
        [Alias("ResourceGroup")]
        [Parameter(ParameterSetName = AzureName, Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        // -ServerName
        [Alias("Server")]
        [Parameter(ParameterSetName = GenericName, Position = 0,                   ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        // -DatabaseName
        [Alias("Database")]
        [Parameter(ParameterSetName = GenericName, Position = 1,                   ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 3, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        // -Credential
        [Parameter(ParameterSetName = GenericName, Position = 2,                   ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 4, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Credential]
        public PSCredential Credential { get; set; } = PSCredential.Empty;

        // -UseAzureActiveDirectoryPassword
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter UseAzureActiveDirectoryPassword { get; set; }

        // -EncryptionMode
        [Alias("Encryption")]
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        public EncryptionMode EncryptionMode { get; set; }

        // -ReadOnlyIntent
        [Alias("ReadOnly")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter ReadOnlyIntent { get; set; }

        // -ClientName
        [Alias("Client")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ClientName { get; set; }

        // -ApplicationName
        [Alias("Application")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ApplicationName { get; set; }

        // -ConnectTimeout
        [Alias("Timeout")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateRange("0:00:00", "24855.03:14:07")]
        public TimeSpan? ConnectTimeout { get; set; }

        // -PersistSecurityInfo
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public Boolean PersistSecurityInfo { get; set; } = true;

        // -Pooling
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public Boolean Pooling { get; set; }  = true;

        // -MultipleActiveResultSets
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public Boolean MultipleActiveResultSets { get; set; }  = true;

        // -TrustServerCertificate
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public Boolean TrustServerCertificate { get; set; }  = true;


        protected override void ProcessRecord()
        {
            var context = Azure.IsPresent
                ? new AzureSqlContext { ResourceGroupName = ResourceGroupName }
                : UseAzureActiveDirectoryPassword.IsPresent
                 ? new AzureActiveDirectorySqlContext { 
                     EncryptionMode = EncryptionMode, 
                     PersistSecurityInfo = PersistSecurityInfo,
                     Pooling = Pooling,
                     MultipleActiveResultSets = MultipleActiveResultSets,
                     TrustServerCertificate = TrustServerCertificate
                   }
                : new SqlContext      { EncryptionMode    = EncryptionMode    };

            var credential = Credential.IsNullOrEmpty()
                ? null
                : Credential;

            context.ServerName        = ServerName;
            context.DatabaseName      = DatabaseName;
            context.Credential        = credential;
            context.ConnectTimeout    = ConnectTimeout;
            context.ClientName        = ClientName;
            context.ApplicationName   = ApplicationName;
            context.ApplicationIntent = ReadOnlyIntent ? ReadOnly : ReadWrite;

            WriteObject(context);
        }
    }
}
