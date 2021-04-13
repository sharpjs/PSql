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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSql
{
    /// <summary>
    ///   Information necessary to connect to an Azure SQL Database or
    ///   compatible database.
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
        ///   Initializes a new <see cref="AzureSqlContext"/> instance by
        ///   property values from the specified instance.
        /// </summary>
        /// <param name="other">
        ///   The instance from which to copy property values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="other"/> is <c>null</c>.
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
        ///   virtual database server.  The default is <c>null</c>.
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
        ///   Gets or sets the Azure resource name of the virtual database
        ///   server.  The default is <c>null</c>.
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
        ///   Gets or sets the method used to authenticate with the database
        ///   server.  The default is
        ///   <see cref="AzureAuthenticationMode.Default"/>.
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
        ///   Gets a new context that is a copy of the current instance, but
        ///   with the specified server resource group name, server resource
        ///   name, and database name.  If the current instance is frozen, the
        ///   copy is frozen also.
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

            if (string.IsNullOrEmpty(ServerResourceGroupName) ||
                string.IsNullOrEmpty(ServerResourceName))
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

            if (string.IsNullOrEmpty(value))
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

        protected override void ConfigureServerName(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            // Ignore ServerPort and InstanceName.

            builder.DataSource = GetEffectiveServerName();
        }

        protected override void ConfigureDefaultDatabaseName(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            if (!string.IsNullOrEmpty(DatabaseName))
                builder.InitialCatalog = DatabaseName;
            else
                builder.InitialCatalog = MasterDatabaseName;
        }

        protected override void ConfigureAuthentication(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            var mode = AuthenticationMode;

            switch (mode)
            {
                case AzureAuthenticationMode.Default when Credential.IsNullOrEmpty():
                    mode = AzureAuthenticationMode.AadIntegrated;
                    break;

                case AzureAuthenticationMode.Default:
                    mode = AzureAuthenticationMode.SqlPassword;
                    break;

                case AzureAuthenticationMode.SqlPassword:
                case AzureAuthenticationMode.AadPassword:
                case AzureAuthenticationMode.AadServicePrincipal:
                    if (Credential.IsNullOrEmpty())
                        throw new NotSupportedException("A credential is required when connecting to Azure SQL Database.");
                    break;
            }

            builder.Authentication = PSqlClient.Instance.GetAuthenticationMethod((int) mode);

            if (!Credential.IsNullOrEmpty() && ExposeCredentialInConnectionString)
            {
                builder.UserID              = Credential.UserName;
                builder.Password            = Credential.GetNetworkCredential().Password;
                builder.PersistSecurityInfo = true;
            }
        }

        protected override void ConfigureEncryption(
            dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            // Encryption is required for connections to Azure SQL Database
            builder.Encrypt = true;

            // Always verify server identity
            // builder.TrustServerCertificate defaults to false
        }
    }
}
