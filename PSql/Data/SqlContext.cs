// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

// TODO: Document
#pragma warning disable CS1591

using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using PSql.Commands;

namespace PSql;

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
    ///   <paramref name="other"/> is <see langword="null"/>.
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
    ///   context is a non-Azure context, this property is
    ///   <see langword="null"/>.
    /// </summary>
    public AzureSqlContext? AsAzure => this as AzureSqlContext;

    /// <summary>
    ///   Gets whether the context connects to the local computer.
    /// </summary>
    public bool IsLocal => GetIsLocal();

    /// <summary>
    ///   Gets whether the context is frozen.  A frozen context is read-only;
    ///   its properties cannot be changed.
    /// </summary>
    public bool IsFrozen => _isFrozen;

    /// <summary>
    ///   Gets or sets the DNS name of the database server.  The values
    ///   <c>.</c> and <c>(local)</c> are recognized as aliases for the local
    ///   machine.  If <see langword="null"/> or an empty string, behavior is
    ///   context-dependent: <see cref="SqlContext"/> connects to the local
    ///   machine, and <see cref="AzureSqlContext"/> connects to the Azure
    ///   virtual database server identified by the
    ///     <see cref="AzureSqlContext.ServerResourceGroupName"/> and
    ///     <see cref="AzureSqlContext.ServerResourceName"/> properties.
    ///   The default is <see langword="null"/>.
    /// </summary>
    public string? ServerName
    {
        get => _serverName;
        set => Set(out _serverName, value);
    }

    /// <summary>
    ///   Gets or sets the remote TCP port of the database server.  If
    ///   <see langword="null"/>, the underlying ADO.NET implementation will
    ///   use a default port, typically 1433.  The default is
    ///   <see langword="null"/>.
    /// </summary>
    public ushort? ServerPort
    {
        get => _serverPort;
        set => Set(out _serverPort, value);
    }

    /// <summary>
    ///   Gets or sets the name of the database engine instance.  If
    ///   <see langword="null"/>, connection attempts will target the default
    ///   instance.  The default is <see langword="null"/>.
    /// </summary>
    public string? InstanceName
    {
        get => _instanceName;
        set => Set(out _instanceName, value);
    }

    /// <summary>
    ///  Gets or sets the name of the database.  If <see langword="null"/>,
    ///  connections will attempt to open in the default database of the
    ///  authenticated user.  The default is <see langword="null"/>.
    /// </summary>
    public string? DatabaseName
    {
        get => _databaseName;
        set => Set(out _databaseName, value);
    }

    /// <summary>
    ///   Gets or sets the credential to use to authenticate with the database
    ///   server.  The default is <see langword="null"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     For <see cref="AzureSqlContext"/>, the use of this property
    ///     depends on the authentication mode.  For details, see the
    ///     <see cref="AzureSqlContext.AuthenticationMode"/> property.
    ///   </para>
    ///   <para>
    ///     For non-Azure contexts, if <c>Credential</c> is not
    ///     <see langword="null"/>, connections will use SQL passwor
    ///     authentication.  If this property is <see langword="null"/>,
    ///     connections will use integrated authentication.
    ///   </para>
    /// </remarks>
    public PSCredential? Credential
    {
        get => _credential;
        set => Set(out _credential, value);
    }

    /// <summary>
    ///   Gets or sets a value that specifies the transport encryption to use
    ///   for connections.  The default is <see cref="EncryptionMode.Default"/>.
    /// </summary>
    public virtual EncryptionMode EncryptionMode
    {
        get => _encryptionMode;
        set => Set(out _encryptionMode, value);
    }

    /// <summary>
    ///   Gets or sets the duration after which a connection attempt times
    ///   out.  If <see langword="null"/>, the underlying ADO.NET
    ///   implementation default of 15 seconds is used.
    /// </summary>
    public TimeSpan? ConnectTimeout
    {
        get => _connectTimeout;
        set => Set(out _connectTimeout, value);
    }

    /// <summary>
    ///   Gets or sets the name of the client device.  If <see langword="null"/>,
    ///   the underlying ADO.NET implementation will provide a default value.
    /// </summary>
    public string? ClientName
    {
        get => _clientName;
        set => Set(out _clientName, value);
    }

    /// <summary>
    ///   Gets or sets the name of the client application.  If <see langword="null"/>,
    ///   the underlying ADO.NET implementation will provide a default value.
    /// </summary>
    public string? ApplicationName
    {
        get => _applicationName;
        set => Set(out _applicationName, value);
    }

    /// <summary>
    ///   Gets or sets a value that declares the kinds of operations that the
    ///   client application intends to perform against databases.  The
    ///   default is <see cref="ApplicationIntent.ReadWrite"/>.
    /// </summary>
    public ApplicationIntent ApplicationIntent
    {
        get => _applicationIntent;
        set => Set(out _applicationIntent, value);
    }

    /// <summary>
    ///   Gets or sets whether the credential used for authentication should
    ///   be exposed in the <see cref="DbConnection.ConnectionString"/>
    ///   property.  This is a potential security risk, so use only when
    ///   necessary.  The default is <see langword="false"/>.
    /// </summary>
    public bool ExposeCredentialInConnectionString
    {
        get => _exposeCredentialInConnectionString;
        set => Set(out _exposeCredentialInConnectionString, value);
    }

    /// <summary>
    ///   Gets or sets whether connections may be pooled to reduce setup and
    ///   teardown time.  Pooling is useful when making many connections with
    ///   identical connection strings.  The default is <see langword="false"/>.
    /// </summary>
    public bool EnableConnectionPooling
    {
        get => _enableConnectionPooling;
        set => Set(out _enableConnectionPooling, value);
    }

    /// <summary>
    ///   Gets or sets whether connections support execution of multiple
    ///   batches concurrently, with limitations.  The default is
    ///   <see langword="false"/>.  For more information, see
    ///   <a href="https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/multiple-active-result-sets-mars"
    ///   >Multiple Active Result Sets (MARS)</a>.
    /// </summary>
    public bool EnableMultipleActiveResultSets
    {
        get => _enableMultipleActiveResultSets;
        set => Set(out _enableMultipleActiveResultSets, value);
    }

    /// <summary>
    ///   Gets a new context that is a copy of the current instance, then
    ///   modified by the specified script block.  If the current instance is
    ///   frozen, the copy becomes frozen after the script block ends.
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
    ///   Gets a new context that is a copy of the current instance, but with
    ///   the specified database name.  If the current instance is frozen, the
    ///   copy is frozen also.
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
    ///   Gets a new context that is a copy of the current instance, but with
    ///   the specified server name and database name.  If the current
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
    ///   Freezes the context if it is not frozen already.  Once frozen, the
    ///   properties of the context cannot be changed.
    /// </summary>
    public SqlContext Freeze()
    {
        _isFrozen = true;
        return this;
    }

    /// <summary>
    ///   Returns the effective DNS name of the database server.  If the
    ///   <see cref="ServerName"/> property is neither <see langword="null"/>
    ///   nor empty, this method returns the property value.  Otherwise,
    ///   behavior is context-dependent.  For <see cref="SqlContext"/>, this
    ///   method returns an alias for the local machine.  For
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

    public NetworkCredential? GetNetworkCredential()
    {
        return Credential.IsNullOrEmpty()
            ? null
            : Credential.GetNetworkCredential();
    }

    /// <summary>
    ///   Gets a connection string built from the property values of the
    ///   current context, optionally with the specified database name,
    ///   compatibility, and/or credential disclosure.
    /// </summary>
    /// <param name="databaseName">
    ///   The name of the database to specify in the connection string.  If
    ///   not <see langword="null"/>, this parameter overrides the value of
    ///   the <see cref="DatabaseName"/> property.  The default is
    ///   <see langword="null"/>.
    /// </param>
    /// <param name="sqlClientVersion">
    ///   The SqlClient version with which the generated connection string
    ///   should be compatible.  The default is
    ///   <see cref="SqlClientVersion.Legacy"/>.
    /// </param>
    /// <param name="omitCredential">
    ///   <para>
    ///     Whether to omit credential properties from the generated
    ///     connection string if possible.  Use <see langword="true"/> when
    ///     passing the credential separately from the connection string (for
    ///     example, as a <c>SqlCredential</c> object).  The default is
    ///     <see langword="false"/>.
    ///   </para>
    ///   <para>
    ///     Note that the <see cref="ExposeCredentialInConnectionString"/>
    ///     takes precedence over this this parameter.  If the property is
    ///     <see langword="true"/>, the generated connection string will
    ///     include credential properties regardless of this parameter.
    ///   </para>
    /// </param>
    /// <returns>
    ///   A connection string compatible with <paramref name="sqlClientVersion"/>
    ///   built from the property values of the current context and, if
    ///   specified, <paramref name="databaseName"/>.  If
    ///   <paramref name="omitCredential"/> is <see langword="true"/>, the
    ///   connection string will not include credential properties.
    /// </returns>
    public string GetConnectionString(
        string?           databaseName     = null,
        SqlClientVersion  sqlClientVersion = SqlClientVersion.Legacy,
        bool              omitCredential   = false)
    {
        if (databaseName.IsNullOrEmpty())
            databaseName = DatabaseName;

        var builder = new SqlConnectionStringBuilder(sqlClientVersion);

        ConfigureServerName     (builder);
        ConfigureDatabaseName   (builder, databaseName);
        ConfigureAuthentication (builder, omitCredential);
        ConfigureEncryption     (builder);

        // Connect Timeout
        if (ConnectTimeout.HasValue)
            builder.AppendConnectTimeout(ConnectTimeout.Value);

        // Client Name
        if (ClientName.HasContent())
            builder.AppendClientName(ClientName);

        // Application Name
        if (ApplicationName.HasContent())
            builder.AppendApplicationName(ApplicationName);

        // Application Intent
        if (ApplicationIntent != ApplicationIntent.ReadWrite)
            builder.AppendApplicationIntent(ApplicationIntent);

        // Multiple Active Result Sets
        if (EnableMultipleActiveResultSets)
            builder.AppendMultipleActiveResultSets(true);

        // Connection Pooling
        if (!EnableConnectionPooling)
            builder.AppendPooling(false);

        return builder.ToString();
    }

    private protected virtual void
        ConfigureServerName(SqlConnectionStringBuilder builder)
    {
        var serverName = GetEffectiveServerName();

        if (ServerPort.HasValue || InstanceName.HasContent())
        {
            var s = new StringBuilder(serverName);

            if (InstanceName.HasContent())
                s.Append('\\').Append(InstanceName);

            if (ServerPort.HasValue)
                s.Append(',').Append(ServerPort.Value.ToString(CultureInfo.InvariantCulture));

            serverName = s.ToString();
        }

        builder.AppendServerName(serverName);
    }

    private protected virtual void
        ConfigureDatabaseName(SqlConnectionStringBuilder builder, string? databaseName)
    {
        if (databaseName.HasContent())
            builder.AppendDatabaseName(databaseName);
        //else
        //  server determines database
    }

    private protected virtual void
        ConfigureAuthentication(SqlConnectionStringBuilder builder, bool omitCredential)
    {
        if (Credential.IsNullOrEmpty())
        {
            builder.AppendIntegratedSecurity(true);
        }
        else if (!omitCredential || ExposeCredentialInConnectionString)
        {
            builder.AppendCredential(Credential.GetNetworkCredential());

            if (ExposeCredentialInConnectionString)
                builder.AppendPersistSecurityInfo(true);
        }
        //else
        //  will provide credential as a SqlCredential object
    }

    private protected virtual void
        ConfigureEncryption(SqlConnectionStringBuilder builder)
    {
        var (encrypt, verifyServerIdentity)
            = TranslateEncryptionMode(EncryptionMode);

        if (encrypt != builder.Version.GetDefaultEncrypt())
            builder.AppendEncrypt(encrypt);

        if (!verifyServerIdentity)
            builder.AppendTrustServerCertificate(true);
    }

    private (bool, bool) TranslateEncryptionMode(EncryptionMode mode)
    {
        // tuple: (encrypt, verifyServerIdentity)

        return mode switch
        {
            //                                     ( ENCRYPT, VERIFY )
            EncryptionMode.None                 => ( false,   false  ),
            EncryptionMode.Unverified           => ( true,    false  ),
            EncryptionMode.Full                 => ( true,    true   ),
            EncryptionMode.Default when IsLocal => ( false,   false  ),
            _                                   => ( true,    true   ),
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

#if FALSE
    /// <summary>
    ///   Opens a connection as determined by the property values of the
    ///   current context, optionally with the specified database name, logging
    ///   server messages with the specified logger.
    /// </summary>
    /// <param name="databaseName">
    ///   A database name.  If not <see langword="null"/>, this parameter
    ///   overrides the value of the <see cref="DatabaseName"/> property.
    /// </param>
    /// <param name="logger">
    ///   The object to use to log server messages received over the
    ///   connection.
    /// </param>
    /// <returns>
    ///   An object representing the open connection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    public ISqlConnection Connect(string? databaseName, ISqlMessageLogger logger)
    {
        const SqlClientVersion Version = SqlClientVersion.Latest;

        var connectionString = GetConnectionString(databaseName, Version, true);
        var credential       = Credential;

        var passCredentialSeparately
            =  !credential.IsNullOrEmpty()
            && !ExposeCredentialInConnectionString;

        return passCredentialSeparately
            ? new SqlConnection(
                connectionString,
                credential!.UserName,
                credential!.Password,
                logger
            )
            : new SqlConnection(
                connectionString,
                logger
            );
    }

    public ISqlConnection Connect(string? databaseName, Cmdlet cmdlet)
    {
        return Connect(databaseName, new CmdletSqlMessageLogger(cmdlet));
    }
#endif

    /// <summary>
    ///   Opens a connection as determined by the property values of the
    ///   current context, optionally to the specified database, logging server
    ///   messages via the specified cmdlet.
    /// </summary>
    /// <param name="databaseName">
    ///   An optional database name.  If not <see langword="null"/>, this
    ///   parameter overrides the value of the <see cref="DatabaseName"/>
    ///   property, and this methods attempt to connect to the specified
    ///   database.
    /// </param>
    /// <param name="cmdlet">
    ///   The PowerShell cmdlet to use to log server messages received over the
    ///   connection.
    /// </param>
    /// <returns>
    ///   An object representing the open connection.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    internal SqlConnection Connect(string? databaseName, ICmdlet cmdlet)
    {
        const SqlClientVersion Version = SqlClientVersion.Latest;

        var connectionString = GetConnectionString(databaseName, Version, true);
        var credential       = Credential;

        var passCredentialSeparately
            =  !credential.IsNullOrEmpty()
            && !ExposeCredentialInConnectionString;

        return new(
            connectionString,
            passCredentialSeparately ? credential!.GetNetworkCredential() : null,
            cmdlet
        );
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
