using System.Data.SqlClient;
using System.Management.Automation;

namespace PSql
{
    public class SqlContext
    {
        protected const string
            LocalServerName    = ".",
            MasterDatabaseName = "master";

        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public PSCredential Credential { get; set; }

        public bool UseEncryption { get; set; } = true;

        public bool UseServerIdentityCheck { get; set; } = true;

        public int? ConnectionTimeoutSeconds { get; set; }

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

            // Authentication
            if (Credential == null || Credential == PSCredential.Empty)
                builder.IntegratedSecurity = true;

            // Encryption
            if (UseEncryption)
                builder.Encrypt = true;

            // Server Identity Check
            if (UseEncryption && !UseServerIdentityCheck)
                builder.TrustServerCertificate = true;

            // Timeout
            if (ConnectionTimeoutSeconds.HasValue)
                builder.ConnectTimeout = ConnectionTimeoutSeconds.Value;

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

        private SqlCredential GetCredential()
        {
            if (Credential == null || Credential == PSCredential.Empty)
                return null; // using integrated security

            return new SqlCredential(Credential.UserName, Credential.Password);
        }
    }
}
