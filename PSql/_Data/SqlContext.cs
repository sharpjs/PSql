using System;
using System.Management.Automation;
using System.Net;
using Microsoft.Data.SqlClient;

namespace PSql
{
    /// <summary>
    ///   Information necessary to connect to a SQL Server or compatible
    ///   database.
    /// </summary>
    public class SqlContext
    {
        protected const string
            LocalServerName    = ".",
            MasterDatabaseName = "master";
 
        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public PSCredential Credential { get; set; }

        public EncryptionMode EncryptionMode { get; set; }

        public TimeSpan? ConnectTimeout { get; set; }

        public string ClientName { get; set; }

        public string ApplicationName { get; set; }

        public ApplicationIntent ApplicationIntent { get; set; }

        public bool ExposeCredentialInConnectionString { get; set; }

        public bool EnableConnectionPooling { get; set; }

        public bool EnableMultipleActiveResultSets { get; set; }

        public virtual bool IsAzure => false;

        public AzureSqlContext AsAzure => this as AzureSqlContext;

        public bool IsLocal => GetIsLocal();

        internal SqlConnection CreateConnection(string databaseName)
        {
            var builder = new SqlConnectionStringBuilder();

            BuildConnectionString(builder);

            if (databaseName != null)
                builder.InitialCatalog = databaseName;

            var connectionString = builder.ToString();
            var credential       = GetCredential();

            return credential == null
                ? new SqlConnection(connectionString)
                : new SqlConnection(connectionString, credential);
        }

        protected virtual void BuildConnectionString(SqlConnectionStringBuilder builder)
        {
            // Server
            if (!string.IsNullOrEmpty(ServerName))
                builder.DataSource = ServerName;
            else
                builder.DataSource = LocalServerName;

            // Database
            if (!string.IsNullOrEmpty(DatabaseName))
                builder.InitialCatalog = DatabaseName;
            //else
            //  server determines database

            // Security
            ConfigureAuthentication (builder);
            ConfigureEncryption     (builder);

            // Timeout
            if (ConnectTimeout.HasValue)
                builder.ConnectTimeout = ConnectTimeout.Value.GetAbsoluteSecondsSaturatingInt32();

            // Client Name
            if (!string.IsNullOrEmpty(ClientName))
                builder.WorkstationID = ClientName;

            // Application Name
            if (!string.IsNullOrEmpty(ApplicationName))
                builder.ApplicationName = ApplicationName;

            // Application Intent
            if (ApplicationIntent != ApplicationIntent.ReadWrite)
                builder.ApplicationIntent = ApplicationIntent;

            // Other
            builder.PersistSecurityInfo      = ExposeCredentialInConnectionString;
            builder.MultipleActiveResultSets = EnableMultipleActiveResultSets;
            builder.Pooling                  = EnableConnectionPooling;
        }

        protected virtual void ConfigureAuthentication(SqlConnectionStringBuilder builder)
        {
            // Authentication
            if (Credential.IsNullOrEmpty())
                builder.IntegratedSecurity = true;
            //else
            //  will provide credential as a SqlCredential object
        }

        protected virtual void ConfigureEncryption(SqlConnectionStringBuilder builder)
        {
            var (useEncryption, useServerIdentityCheck)
                = TranslateEncryptionMode(EncryptionMode);

            if (useEncryption)
                builder.Encrypt = true;

            if (!useServerIdentityCheck)
                builder.TrustServerCertificate = true;
        }

        private (bool, bool) TranslateEncryptionMode(EncryptionMode mode)
        {
            // tuple: (useEncryption, useServerIdentityCheck)

            switch (mode)
            {
                //                                     ( ENCRYPT, VERIFY )
                case EncryptionMode.None:       return ( false,   false  );
                case EncryptionMode.Unverified: return ( true,    false  );
                case EncryptionMode.Full:       return ( true,    true   );
                case EncryptionMode.Default:
                default:
                    var isRemote = !GetIsLocal();
                    return (isRemote, isRemote);
            }
        }

        private bool GetIsLocal()
        {
            if (string.IsNullOrEmpty(ServerName))
                return true;

            var comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Equals(ServerName, LocalServerName)
                || comparer.Equals(ServerName, "(local)")
                || comparer.Equals(ServerName, "localhost")
                || comparer.Equals(ServerName, Dns.GetHostName());
        }

        private SqlCredential GetCredential()
        {
            if (Credential.IsNullOrEmpty())
                return null; // using integrated security

            // Prevent error that occurs if password is not marked read-only
            var password = Credential.Password;
            if (!password.IsReadOnly())
                (password = password.Copy()).MakeReadOnly();

            return new SqlCredential(Credential.UserName, password);
        }
    }
}
