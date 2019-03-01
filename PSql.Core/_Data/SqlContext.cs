using System;
using System.Data.SqlClient;
using System.Management.Automation;
using System.Net;

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

        internal SqlConnection CreateConnection()
        {
            var builder = new SqlConnectionStringBuilder();

            BuildConnectionString(builder);

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

            // Authentication
            if (Credential.IsNullOrEmpty())
                builder.IntegratedSecurity = true;
            //else
            //  will provide credential as a SqlCredential object

            // Encryption & Server Identity Check
            ConfigureEncryption(builder);

            // Timeout
            if (ConnectTimeout >= TimeSpan.Zero)
                builder.ConnectTimeout = ConnectTimeout.Value.GetTotalSecondsSaturatingInt32();

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
            builder.Pooling = false;
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
            switch (mode)
            {
                case EncryptionMode.None:       return (false, false);
                case EncryptionMode.Unverified: return (true,  false);
                case EncryptionMode.Full:       return (true,  true );
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

            return new SqlCredential(Credential.UserName, Credential.Password);
        }
    }
}
