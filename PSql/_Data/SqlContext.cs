/*
    Copyright 2021 Jeffrey Sharp

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
using System.DirectoryServices;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using System.Runtime.CompilerServices;
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

        private bool              _isFrozen;
        private string?           _serverName;
        private ushort?           _serverPort;
        private string?           _instanceName;
        private string?           _databaseName;
        private PSCredential?     _credential;
        private EncryptionMode    _encryptionMode;
        private TimeSpan?         _connectTimeout;
        private string?           _clientName;
        private string?           _applicationName;
        private ApplicationIntent _applicationIntent;
        private bool              _exposeCredentialInConnectionString;
        private bool              _enableConnectionPooling;
        private bool              _enableMultipleActiveResultSets;

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

            _serverName                         = other.ServerName;
            _serverPort                         = other.ServerPort;
            _instanceName                       = other.InstanceName;
            _databaseName                       = other.DatabaseName;
            _credential                         = other.Credential;
            _encryptionMode                     = other.EncryptionMode;
            _connectTimeout                     = other.ConnectTimeout;
            _clientName                         = other.ClientName;
            _applicationName                    = other.ApplicationName;
            _applicationIntent                  = other.ApplicationIntent;
            _exposeCredentialInConnectionString = other.ExposeCredentialInConnectionString;
            _enableConnectionPooling            = other.EnableConnectionPooling;
            _enableMultipleActiveResultSets     = other.EnableMultipleActiveResultSets;
        }

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
        ///   Gets whether the context is frozen.  A frozen context is
        ///   read-only; its properties cannot be changed.
        /// </summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>
        ///   Gets or sets the DNS name of the database server.  The values
        ///   <c>.</c> and <c>(local)</c> are recognized as aliases for the
        ///   local machine.  If <c>null</c> or an empty string, behavior is
        ///   context-dependent: <see cref="SqlContext"/> connects to the local
        ///   machine, and <see cref="AzureSqlContext"/> connects to the Azure
        ///   virtual database server identified by the
        ///     <see cref="AzureSqlContext.ServerResourceGroupName"/> and
        ///     <see cref="AzureSqlContext.ServerResourceName"/> properties.
        ///   The default is <c>null</c>.
        /// </summary>
        public string? ServerName
        {
            get => _serverName;
            set => Set(out _serverName, value);
        }

        /// <summary>
        ///   Gets or sets the remote TCP port of the database server.  If
        ///   <c>null</c>, the underlying ADO.NET implementation will use a
        ///   default port, typically 1433.  The default is <c>null</c>.
        /// </summary>
        public ushort? ServerPort
        {
            get => _serverPort;
            set => Set(out _serverPort, value);
        }

        /// <summary>
        ///   Gets or sets the name of the database engine instance.  If
        ///   <c>null</c>, connection attempts will target the default
        ///   instance.  The default is <c>null</c>.
        /// </summary>
        public string? InstanceName
        {
            get => _instanceName;
            set => Set(out _instanceName, value);
        }

        /// <summary>
        ///  Gets or sets the name of the database.  If <c>null</c>,
        ///  connections will attempt to open in the default database of the
        ///  authenticated user.  The default is <c>null</c>.
        /// </summary>
        public string? DatabaseName
        {
            get => _databaseName;
            set => Set(out _databaseName, value);
        }

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
        public PSCredential? Credential
        {
            get => _credential;
            set => Set(out _credential, value);
        }

        /// <summary>
        ///   Gets or sets a value that specifies the transport encryption to
        ///   use for connections.  The default is
        ///   <see cref="EncryptionMode.Default"/>.
        /// </summary>
        public virtual EncryptionMode EncryptionMode
        {
            get => _encryptionMode;
            set => Set(out _encryptionMode, value);
        }

        /// <summary>
        ///   Gets or sets the duration after which a connection attempt times
        ///   out.  If <c>null</c>, the underlying ADO.NET implementation
        ///   default of 15 seconds is used.
        /// </summary>
        public TimeSpan? ConnectTimeout
        {
            get => _connectTimeout;
            set => Set(out _connectTimeout, value);
        }

        /// <summary>
        ///   Gets or sets the name of the client device.  If <c>null</c>, the
        ///   underlying ADO.NET implementation will provide a default value.
        /// </summary>
        public string? ClientName
        {
            get => _clientName;
            set => Set(out _clientName, value);
        }

        /// <summary>
        ///   Gets or sets the name of the client application.  If <c>null</c>,
        ///   the underlying ADO.NET implementation will provide a default
        ///   value.
        /// </summary>
        public string? ApplicationName
        {
            get => _applicationName;
            set => Set(out _applicationName, value);
        }

        /// <summary>
        ///   Gets or sets a value that declares the kinds of operations that
        ///   the client application intends to perform against databases.  The
        ///   default is <see cref="ApplicationIntent.ReadWrite"/>.
        /// </summary>
        public ApplicationIntent ApplicationIntent
        {
            get => _applicationIntent;
            set => Set(out _applicationIntent, value);
        }

        /// <summary>
        ///   Gets or sets whether the credential used for authentication
        ///   should be exposed in the <see cref="SqlConnection.ConnectionString"/>
        ///   property.  This is a potential security risk, so use only when
        ///   necessary.  The default is <c>false</c>.
        /// </summary>
        public bool ExposeCredentialInConnectionString
        {
            get => _exposeCredentialInConnectionString;
            set => Set(out _exposeCredentialInConnectionString, value);
        }

        /// <summary>
        ///   Gets or sets whether connections may be pooled to reduce setup
        ///   and teardown time.  Pooling is useful when making many
        ///   connections with identical connection strings.  The default is
        ///   <c>false</c>.
        /// </summary>
        public bool EnableConnectionPooling
        {
            get => _enableConnectionPooling;
            set => Set(out _enableConnectionPooling, value);
        }

        /// <summary>
        ///   Gets or sets whether connections support execution of multiple
        ///   batches concurrently, with limitations.  The default is <c>false</c>.
        ///   For more information, see Multiple Active Result Sets (MARS):
        ///   https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/multiple-active-result-sets-mars
        /// </remarks>
        public bool EnableMultipleActiveResultSets
        {
            get => _enableMultipleActiveResultSets;
            set => Set(out _enableMultipleActiveResultSets, value);
        }

        /// <summary>
        ///   Gets a new context that is a copy of the current instance, then
        ///   modified by the specified script block.  If the current instance
        ///   is frozen, the copy becomes frozen after the script block ends.
        /// </summary>
        /// <param name="block">
        ///   A script block that can modify the created context.  Inside the
        ///   script block, the variable <c>$_</c> holds the created context.
        /// </param>
        public SqlContext this[ScriptBlock block]
            => CloneAndModify(this, clone =>
            {
                if (block is null)
                    throw new ArgumentNullException(nameof(block));

                block.InvokeWithUnderscore(clone);
            });

        /// <summary>
        ///   Gets a new context that is a copy of the current instance, but
        ///   with the specified database name.  If the current instance is
        ///   frozen, the copy is frozen also.
        /// </summary>
        /// <param name="databaseName">
        ///   The name of the database to set on the copy.
        /// </param>
        public SqlContext this[string? databaseName]
            => CloneAndModify(this, clone =>
            {
                clone.DatabaseName = databaseName;
            });

        /// <summary>
        ///   Gets a new context that is a copy of the current instance, but
        ///   with the specified server name and database name.  If the current
        ///   instance is frozen, the copy is frozen also.
        /// </summary>
        /// <param name="serverName">
        ///   The name of the server to set on the copy.
        /// </param>
        /// <param name="databaseName">
        ///   The name of the database to set on the copy.
        /// </param>
        public SqlContext this[string? serverName, string? databaseName]
            => CloneAndModify(this, clone =>
            {
                clone.ServerName   = serverName;
                clone.DatabaseName = databaseName;
            });

        /// <summary>
        ///   Creates a new, non-frozen context that is a copy of the current
        ///   instance.
        /// </summary>
        public SqlContext Clone()
            => CloneCore();

        /// <inheritdoc/>
        object ICloneable.Clone()
            => CloneCore();

        /// <summary>
        ///   Creates a new, non-frozen context that is a copy of the current
        ///   instance.  Subclasses should override this method.
        /// </summary>
        protected virtual SqlContext CloneCore()
            => new(this);

        /// <summary>
        ///   Freezes the context if it is not frozen already.  Once frozen,
        ///   the properties of the context cannot be changed.
        /// </summary>
        public SqlContext Freeze()
        {
            _isFrozen = true;
            return this;
        }

        /// <summary>
        ///   Returns the effective DNS name of the database server.  If the
        ///   <see cref="ServerName"/> property is neither <c>null</c> nor
        ///   empty, this method returns the property value.  Otherwise,
        ///   behavior is context-dependent.  For <see cref="SqlContext"/>,
        ///   this method returns an alias for the local machine.  For
        ///   <see cref="AzureSqlContext"/>, this method returns the DNS name
        ///   of the Azure virtual database server identified by the
        ///     <see cref="AzureSqlContext.ServerResourceGroupName"/> and
        ///     <see cref="AzureSqlContext.ServerResourceName"/> properties.
        /// </summary>
        public string GetEffectiveServerName()
        {
            return ServerName.NullIfEmpty() ?? GetDefaultServerName();
        }

        private protected virtual string GetDefaultServerName()
            => LocalServerName;

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

            // Enable Multiple Active Result Sets
            if (EnableMultipleActiveResultSets)
                builder.MultipleActiveResultSets = true;

            // Enable Connection Pooling
            if (!EnableConnectionPooling)
                builder.Pooling = false;
        }

        protected virtual void ConfigureServerName(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            var dataSource = GetEffectiveServerName();

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
            {
                builder.IntegratedSecurity = true;
            }
            else if (ExposeCredentialInConnectionString)
            {
                builder.UserID              = Credential.UserName;
                builder.Password            = Credential.GetNetworkCredential().Password;
                builder.PersistSecurityInfo = true;
            }
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

            return mode switch
            {
                //                            ( ENCRYPT,       VERIFY )
                EncryptionMode.None        => ( false,         false  ),
                EncryptionMode.Unverified  => ( true,          false  ),
                EncryptionMode.Full        => ( true,          true   ),
            //  EncryptionMode.Default     ↓↓ ( !GetIsLocal(), true   ),
                _                          => ( !GetIsLocal(), true   ),
            };
        }

        private bool GetIsLocal()
        {
            if (IsAzure)
                return false;

            if (ServerName is null)
                return true;

            var comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Equals(ServerName, string.Empty)
                || comparer.Equals(ServerName, LocalServerName)
                || comparer.Equals(ServerName, "(local)")
                || comparer.Equals(ServerName, "localhost")
                || comparer.Equals(ServerName, "127.0.0.1")
                || comparer.Equals(ServerName, "::1")
                || comparer.Equals(ServerName, Dns.GetHostName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Set<T>(out T slot, T value)
        {
            if (IsFrozen)
                throw OnAttemptToModifyFrozenContext();

            slot = value;
        }

        private protected static T CloneAndModify<T>(T context, Action<T> action)
            where T : SqlContext
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            var clone = (T) context.Clone();

            action(clone);

            if (context.IsFrozen)
                clone.Freeze();

            return clone;
        }

        private static Exception OnAttemptToModifyFrozenContext()
        {
            return new InvalidOperationException(
                "The context is frozen and cannot be modified. " +
                "Create a copy and modify the copy instead."
            );
        }
    }
}
