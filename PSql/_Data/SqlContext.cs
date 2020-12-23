/*
    Copyright 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace PSql
{
    using static FormattableString;

    /// <summary>
    ///   Information necessary to connect to a SQL Server or compatible
    ///   database.
    /// </summary>
    public class SqlContext : ICloneable
    {
        protected const string
            LocalServerName    = ".",
            MasterDatabaseName = "master";

        /// <summary>
        ///   Initializes a new <see cref="SqlContext"/> instance with default
        ///   property values.
        /// </summary>
        public SqlContext() { }

        /// <summary>
        ///   Initializes a new <see cref="SqlContext"/> instance by copying
        ///   property values from the specified instance.
        /// </summary>
        /// <param name="other">
        ///   The instance from which to copy property values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public SqlContext(SqlContext other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            ServerName                         = other.ServerName;
            ServerPort                         = other.ServerPort;
            InstanceName                       = other.InstanceName;
            DatabaseName                       = other.DatabaseName;
            Credential                         = other.Credential;
            EncryptionMode                     = other.EncryptionMode;
            ConnectTimeout                     = other.ConnectTimeout;
            ClientName                         = other.ClientName;
            ApplicationName                    = other.ApplicationName;
            ApplicationIntent                  = other.ApplicationIntent;
            ExposeCredentialInConnectionString = other.ExposeCredentialInConnectionString;
            EnableConnectionPooling            = other.EnableConnectionPooling;
            EnableMultipleActiveResultSets     = other.EnableMultipleActiveResultSets;
        }

        public string ServerName { get; set; }

        public ushort? ServerPort { get; set; }

        public string InstanceName { get; set; }

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

        public SqlContext Clone() => CloneCore();

        object ICloneable.Clone() => CloneCore();

        protected virtual SqlContext CloneCore() => new SqlContext(this);

#if ISOLATED
        internal SqlConnection CreateConnection(string databaseName = null)
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
#endif

        protected void BuildConnectionString(dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            ConfigureServerName          (builder);
            ConfigureDefaultDatabaseName (builder);
            ConfigureAuthentication      (builder);
            ConfigureEncryption          (builder);

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

        protected virtual void ConfigureServerName(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            var dataSource = ServerName.NullIfEmpty() ?? LocalServerName;

            if (ServerPort.HasValue || InstanceName.HasContent())
            {
                var s = new StringBuilder(dataSource);

                if (InstanceName.HasContent())
                    s.Append('\\').Append(InstanceName);

                if (ServerPort.HasValue)
                    s.Append(',').Append(ServerPort.Value.ToString(CultureInfo.InvariantCulture));

                dataSource = s.ToString();
            }

            builder.DataSource = dataSource;
        }

        protected virtual void ConfigureDefaultDatabaseName(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            if (!string.IsNullOrEmpty(DatabaseName))
                builder.InitialCatalog = DatabaseName;
            //else
            //  server determines database
        }

        protected virtual void ConfigureAuthentication(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            // Authentication
            if (Credential.IsNullOrEmpty())
                builder.IntegratedSecurity = true;
            //else
            //  will provide credential as a SqlCredential object
        }

        protected virtual void ConfigureEncryption(
            dynamic /*SqlConnectionStringBuilder*/ builder)
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
            if (ServerName is null)
                return true;

            var comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Equals(ServerName, string.Empty)
                || comparer.Equals(ServerName, LocalServerName)
                || comparer.Equals(ServerName, "(local)")
                || comparer.Equals(ServerName, "localhost")
                || comparer.Equals(ServerName, Dns.GetHostName());
        }

#if ISOLATED
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
#endif
    }
}
