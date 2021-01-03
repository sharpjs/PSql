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
    /// <summary>
    ///   Information necessary to connect to SQL Server, Azure SQL Database,
    ///   or compatible product.
    /// </summary>
    public class SqlContext : ICloneable
    {
        // Connection string defaults are here:
        // https://github.com/dotnet/SqlClient/blob/v2.1.1/src/Microsoft.Data.SqlClient/netcore/src/Microsoft/Data/Common/DbConnectionStringCommon.cs#L690-L731

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

        /// <summary>
        ///   Gets or sets the name (DNS name or Azure resource name) of the
        ///   database server.  The default is <c>null</c>.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     For <see cref="AzureSqlContext"/>, this property is required.
        ///     If <see cref="AzureSqlContext.ResourceGroupName"/> is <c>null</c>,
        ///     this property specifies the DNS name of the Azure SQL Database
        ///     endpoint.  Example: <c>myserver.database.windows.net</c>.
        ///     If <see cref="AzureSqlContext.ResourceGroupName"/> is not
        ///     <c>null</c>, this property specifies the Azure resource name of
        ///     the virtual database server.  Example: <c>myserver</c>.
        ///   </para>
        ///   <para>
        ///     For non-Azure contexts, this parameter is optional and
        ///     specifies the DNS name of the database server.  The values
        ///     <c>.</c> and <c>(local)</c> are recognized as aliases for the
        ///     local machine.  If not specified, connection attempts will
        ///     target the local machine.
        ///   </para>
        /// </remarks>
        public string? ServerName { get; set; }

        /// <summary>
        ///   Gets or sets the remote TCP port of the database server.  If
        ///   <c>null</c>, the underlying ADO.NET implementation will use a
        ///   default port, typically 1433.  The default is <c>null</c>.
        /// </summary>
        public ushort? ServerPort { get; set; }

        /// <summary>
        ///   Gets or sets the name of the database engine instance.  If
        ///   <c>null</c>, connection attempts will target the default
        ///   instance.  The default is <c>null</c>.
        /// </summary>
        public string? InstanceName { get; set; }

        /// <summary>
        ///  Gets or sets the name of the database.  If <c>null</c>,
        ///  connections will attempt to open in the default database of the
        ///  authenticated user.  The default is <c>null</c>.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        ///   Gets or sets the credential to use to authenticate with the
        ///   database server.  The default is <c>null</c>.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     For <see cref="AzureSqlContext"/>, the use of this property
        ///     depends on the authentication mode.  For details, see the
        ///     <see cref="AzureSqlContext.AuthenticationMode"/> property.
        ///   </para>
        ///   <para>
        ///     For non-Azure contexts, if <c>Credential</c> is not
        ///     <c>null</c>, connections will use SQL password authentication.
        ///     If this property is <c>null</c>, connections will use
        ///     integrated authentication.
        ///   </para>
        /// </remarks>
        public PSCredential? Credential { get; set; }

        /// <summary>
        ///   Gets or sets a value that specifies the transport encryption to
        ///   use for connections.  The default is
        ///   <see cref="EncryptionMode.Default"/>.
        /// </summary>
        public EncryptionMode EncryptionMode { get; set; }

        /// <summary>
        ///   Gets or sets the duration after which a connection attempt times
        ///   out.  If <c>null</c>, the underlying ADO.NET implementation
        ///   default of 15 seconds is used.
        /// </summary>
        public TimeSpan? ConnectTimeout { get; set; }

        /// <summary>
        ///   Gets or sets the name of the client device.  If <c>null</c>, the
        ///   underlying ADO.NET implementation will provide a default value.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        ///   Gets or sets the name of the client application.  If <c>null</c>,
        ///   the underlying ADO.NET implementation will provide a default
        ///   value.
        /// </summary>
        public string? ApplicationName { get; set; }

        /// <summary>
        ///   Gets or sets a value that declares the kinds of operations that
        ///   the client application intends to perform against databases.
        /// </summary>
        public ApplicationIntent ApplicationIntent { get; set; }

        /// <summary>
        ///   Gets or sets whether the credential used for authentication
        ///   should be exposed in the <see cref="SqlConnection.ConnectionString"/>
        ///   property.  This is a potential security risk, so use only when
        ///   necessary.
        /// </summary>
        public bool ExposeCredentialInConnectionString { get; set; }

        /// <summary>
        ///   Gets or sets whether connections may be pooled to reduce setup
        ///   and teardown time.  Pooling is useful when making many
        ///   connections with identical connection strings.
        /// </summary>
        public bool EnableConnectionPooling { get; set; }

        /// <summary>
        ///   Gets or sets whether connections support execution of multiple
        ///   batches concurrently, with limitations.  For more information,
        ///   see Multiple Active Result Sets (MARS):
        ///   https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/multiple-active-result-sets-mars
        /// </summary>
        public bool EnableMultipleActiveResultSets { get; set; }

        /// <summary>
        ///   Gets whether the context is an <see cref="AzureSqlContext"/>.
        /// </summary>
        public virtual bool IsAzure => false;

        /// <summary>
        ///   Gets the context cast to <see cref="AzureSqlContext"/>.  If the
        ///   context is a non-Azure context, this property is <c>null</c>.
        /// </summary>
        public AzureSqlContext? AsAzure => this as AzureSqlContext;

        /// <summary>
        ///   Gets whether the context connects to the local computer.
        /// </summary>
        public bool IsLocal => GetIsLocal();

        /// <summary>
        ///    Creates a new object that is a copy of the current instance.
        /// </summary>
        public SqlContext Clone() => CloneCore();

        /// <inheritdoc/>
        object ICloneable.Clone() => CloneCore();

        /// <summary>
        ///    Creates a new object that is a copy of the current instance.
        ///    Subclasses should override this method.
        /// </summary>
        protected virtual SqlContext CloneCore() => new SqlContext(this);

        /// <summary>
        ///   Gets a connection string built from the property values of the
        ///   current context, optionally with the specified database name.
        /// </summary>
        /// <param name="databaseName">
        ///   The name of the database to specify in the connection string.
        ///   If not <c>null</c>, this parameter overrides the value of the
        ///   <see cref="DatabaseName"/> property.
        /// </param>
        /// <returns>
        ///   A connection string built from the property values of the current
        ///   context and, if specified, <paramref name="databaseName"/>.
        /// </returns>
        public string GetConnectionString(string? databaseName = null)
        {
            var builder = PSqlClient.Instance.CreateConnectionStringBuilder();

            BuildConnectionString(builder);

            if (databaseName != null)
                builder.InitialCatalog = databaseName;

            return builder.ToString();
        }

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
                //                                     ( ENCRYPT,       VERIFY )
                case EncryptionMode.None:       return ( false,         false  );
                case EncryptionMode.Unverified: return ( true,          false  );
                case EncryptionMode.Full:       return ( true,          true   );
                case EncryptionMode.Default:    //↓↓↓↓ ( !GetIsLocal(), true   );
                default:                        return ( !GetIsLocal(), true   );
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
    }
}
