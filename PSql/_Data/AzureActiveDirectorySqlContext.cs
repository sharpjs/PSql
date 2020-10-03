using System;
using Microsoft.Data.SqlClient;

namespace PSql
{
    /// <summary>
    ///   Information necessary to connect to an Azure SQL Database or
    ///   compatible database.
    /// </summary>
    public class AzureActiveDirectorySqlContext : SqlContext
    {
        public AzureActiveDirectorySqlContext()
        {
            EncryptionMode = EncryptionMode.Full;
            PersistSecurityInfo = true;
            Pooling = true;
            MultipleActiveResultSets = false;
        }

        public bool PersistSecurityInfo { get; set; }
        public bool Pooling { get; set; }
        public bool MultipleActiveResultSets { get; set; }

        protected override void BuildConnectionString(SqlConnectionStringBuilder builder)
        {
            if (Credential.IsNullOrEmpty())
                throw new NotSupportedException("A credential is required when connecting to Azure SQL Database.");

            base.BuildConnectionString(builder);

            builder.PersistSecurityInfo = PersistSecurityInfo;
            builder.Pooling = Pooling;
            builder.MultipleActiveResultSets = MultipleActiveResultSets;
            builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryPassword;

            if (string.IsNullOrEmpty(DatabaseName))
                builder.InitialCatalog = MasterDatabaseName;
        }

        protected override void ConfigureEncryption(SqlConnectionStringBuilder builder)
        {
            builder.Encrypt = true;
        }
    }
}
