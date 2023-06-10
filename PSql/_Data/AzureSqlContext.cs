// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

using static AzureAuthenticationMode;

/// <summary>
///   Information necessary to connect to an Azure SQL Database or compatible database.
/// </summary>
public class AzureSqlContext : SqlContext
{
    private string?                 _serverResourceGroupName;
    private string?                 _serverResourceName;
    private string?                 _serverResolvedName;
    private AzureAuthenticationMode _authenticationMode;

    /// <summary>
    ///   Initializes a new <see cref="AzureSqlContext"/> instance with
    ///   default property values.
    /// </summary>
    public AzureSqlContext()
    {
        // Encryption is required for connections to Azure SQL Database
        base.EncryptionMode = EncryptionMode.Full;
    }

    /// <summary>
    ///   Initializes a new <see cref="AzureSqlContext"/> instance by property
    ///   values from the specified instance.
    /// </summary>
    /// <param name="other">
    ///   The instance from which to copy property values.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public AzureSqlContext(AzureSqlContext other)
        : base(other)
    {
        _serverResourceGroupName  = other. ServerResourceGroupName;
        _serverResourceName       = other. ServerResourceName;
        _serverResolvedName       = other._serverResolvedName;
        _authenticationMode       = other. AuthenticationMode;
    }

    /// <inheritdoc/>
    public sealed override bool IsAzure => true;

    /// <summary>
    ///   Gets or sets the name of the Azure resource group containing the
    ///   virtual database server.  The default is <see langword="null"/>.
    /// </summary>
    public string? ServerResourceGroupName
    {
        get => _serverResourceGroupName;
        set
        {
            Set(out _serverResourceGroupName, value);
            _serverResolvedName = null;
        }
    }

    /// <summary>
    ///   Gets or sets the Azure resource name of the virtual database server.
    ///   The default is <see langword="null"/>.
    /// </summary>
    public string? ServerResourceName
    {
        get => _serverResourceName;
        set
        {
            Set(out _serverResourceName, value);
            _serverResolvedName = null;
        }
    }

    /// <summary>
    ///   Gets or sets the method used to authenticate with the database server.
    ///   The default is <see cref="AzureAuthenticationMode.Default"/>.
    /// </summary>
    public AzureAuthenticationMode AuthenticationMode
    {
        get => _authenticationMode;
        set => Set(out _authenticationMode, value);
    }

    /// <inheritdoc/>
    public sealed override EncryptionMode EncryptionMode
    {
        get => base.EncryptionMode;
        set { } // Property is immutable for AzureSqlContext
    }

    /// <summary>
    ///   Gets a new context that is a copy of the current instance, but with
    ///   the specified server resource group name, server resource name, and
    ///   database name.  If the current instance is frozen, the copy is
    ///   frozen also.
    /// </summary>
    /// <param name="serverResourceGroupName">
    ///   The value to set on the copy for the name of the Azure resource
    ///   group containing the virtual database server.
    /// </param>
    /// <param name="serverResourceName">
    ///   The value to set on the copy for the Azure resource name of the
    ///   virtual database server.
    /// </param>
    /// <param name="databaseName">
    ///   The value to set on the copy for the name of the database.
    /// </param>
    public AzureSqlContext
        this[
            string? serverResourceGroupName,
            string? serverResourceName,
            string? databaseName
        ]
        => CloneAndModify(this, clone =>
        {
            clone.ServerResourceGroupName = serverResourceGroupName;
            clone.ServerResourceName      = serverResourceName;
            clone.DatabaseName            = databaseName;
        });

    /// <inheritdoc cref="SqlContext.Clone()" />
    public new AzureSqlContext Clone()
        => (AzureSqlContext) CloneCore();

    /// <inheritdoc/>
    protected override SqlContext CloneCore()
        => new AzureSqlContext(this);

    private protected sealed override string GetDefaultServerName()
    {
        // Resolve ServerName using Az module

        if (_serverResolvedName is string existing)
            return existing;

        if (ServerResourceGroupName.IsNullOrEmpty() ||
            ServerResourceName     .IsNullOrEmpty())
        {
            throw new InvalidOperationException(
                "Cannot determine the server DNS name. "                    +
                "Set ServerName to the DNS name of a database server, or "  +
                "set ServerResourceGroupName and ServerResourceName to "    +
                "the resource group name and resource name, respectively, " +
                "of an Azure SQL Database virtual server."
            );
        }

        var value = ScriptBlock
            .Create("param ($x) Get-AzSqlServer @x -ErrorAction Stop")
            .Invoke(new Dictionary<string, object?>
            {
                ["ResourceGroupName"] = ServerResourceGroupName,
                ["ServerName"]        = ServerResourceName,
            })
            .FirstOrDefault()
            ?.Properties["FullyQualifiedDomainName"]
            ?.Value as string;

        if (value.IsNullOrEmpty())
        {
            throw new InvalidOperationException(
                "Failed to determine the server DNS name. "                    +
                "The Get-AzSqlServer command completed without error, "        +
                "but did not yield an object with a FullyQualifiedDomainName " +
                "property set to a non-null, non-empty string."
            );
        }

        return _serverResolvedName = value;
    }

    private protected override void
        ConfigureServerName(SqlConnectionStringBuilder builder)
    {
        // Ignore ServerPort and InstanceName.

        builder.AppendServerName(GetEffectiveServerName());
    }

    private protected override void
        ConfigureDatabaseName(SqlConnectionStringBuilder builder, string? databaseName)
    {
        if (databaseName.IsNullOrEmpty())
            databaseName = MasterDatabaseName;

        builder.AppendDatabaseName(databaseName);
    }

    private protected override void
        ConfigureAuthentication(SqlConnectionStringBuilder builder, bool omitCredential)
    {
        var mode = AuthenticationMode;

        // Mode
        switch (mode)
        {
            case Default when Credential.IsNullOrEmpty():
                mode = AadIntegrated;
                goto default;

            case Default:
                mode = SqlPassword;
                goto case SqlPassword;

            case SqlPassword:
                // No need to specify the mode in connection string in this case
                break;

            default:
                RequireSupport(builder.Version, mode);
                builder.AppendAuthenticationMode(mode);
                break;
        }

        // Credential
        switch (mode)
        {
            // Required
            case SqlPassword:
            case AadPassword:
            case AadServicePrincipal:
                RequireCredential(mode);
                if (!omitCredential || ExposeCredentialInConnectionString)
                    builder.AppendCredential(Credential!.GetNetworkCredential());
                if (ExposeCredentialInConnectionString)
                    builder.AppendPersistSecurityInfo(true);
                break;

            // Optional, password ignored
            case AadManagedIdentity:
            case AadDefault:
                if (!Credential.IsNullOrEmpty())
                    builder.AppendUserName(Credential.UserName);
                break;
        }
    }

    private protected override void
        ConfigureEncryption(SqlConnectionStringBuilder builder)
    {
        // Encryption is required for connections to Azure SQL Database
        builder.AppendEncrypt(true);

        // Always verify server identity
        // builder.AppendTrustServerCertificate(false); is the default
    }

    private void RequireSupport(SqlClientVersion version, AzureAuthenticationMode mode)
    {
        if (version.SupportsAuthenticationMode(mode))
            return;

        var message = string.Format(
            "The specified SqlClient version '{0}' " +
            "does not support authentication mode '{1}'.",
            version, mode
        );

        throw new NotSupportedException(message);
    }

    private void RequireCredential(AzureAuthenticationMode mode)
    {
        if (!Credential.IsNullOrEmpty())
            return;

        var message = string.Format(
            "A credential is required when connecting to " +
            "Azure SQL Database using authentication mode '{0}'.",
            mode
        );

        throw new NotSupportedException(message);
    }
}
