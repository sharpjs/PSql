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
        private string?                 _resourceGroupName;
        private string?                 _serverFullName;
        private AzureAuthenticationMode _authenticationMode;

        /// <summary>
        ///   Initializes a new <see cref="AzureSqlContext"/> instance with
        ///   default property values.
        /// </summary>
        public AzureSqlContext()
        {
            // Encryption is required for connections to Azure SQL Database
            EncryptionMode = EncryptionMode.Full;
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
            _resourceGroupName  = other.ResourceGroupName;
            _serverFullName     = other.ServerFullName;
            _authenticationMode = other.AuthenticationMode;
        }

        /// <inheritdoc/>
        public sealed override bool IsAzure => true;

        /// <summary>
        ///   Gets or sets the name of the resource group containing the
        ///   database server.  The default is <c>null</c>.
        /// </summary>
        public string? ResourceGroupName
        {
            get => _resourceGroupName;
            set => Set(out _resourceGroupName, value);
        }

        /// <summary>
        ///   Gets the DNS name of the database server.  The value is
        ///   <c>null</c> until the context is used to create a connection
        ///   string.
        /// </summary>
        public string? ServerFullName
        {
            get          => _serverFullName;
            internal set => _serverFullName = value;
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

        /// <summary>
        ///   Gets a new context that is a copy of the current instance, but
        ///   with the specified resource group name, server name, and database
        ///   name.  If the current instance is frozen, the copy is frozen also.
        /// </summary>
        /// <param name="resourceGroupName">
        ///   The name of the resource group to set on the copy.
        /// </param>
        /// <param name="serverName">
        ///   The name of the server to set on the copy.
        /// </param>
        /// <param name="databaseName">
        ///   The name of the database to set on the copy.
        /// </param>
        public AzureSqlContext this[string? resourceGroupName, string? serverName, string? databaseName]
            => CloneAndModify(this, clone =>
            {
                clone.ResourceGroupName = resourceGroupName;
                clone.ServerName        = serverName;
                clone.DatabaseName      = databaseName;
            });

        /// <inheritdoc cref="SqlContext.Clone()" />
        public new AzureSqlContext Clone()
            => (AzureSqlContext) CloneCore();

        /// <inheritdoc/>
        protected override SqlContext CloneCore()
            => new AzureSqlContext(this);

        protected override void ConfigureServerName(dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            builder.DataSource = ServerFullName ?? ResolveServerFullName();
        }

        protected override void ConfigureDefaultDatabaseName(dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            if (!string.IsNullOrEmpty(DatabaseName))
                builder.InitialCatalog = DatabaseName;
            else
                builder.InitialCatalog = MasterDatabaseName;
        }

        protected override void ConfigureAuthentication(dynamic /*SqlConnectionStringBuilder*/ builder)
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

        protected override void ConfigureEncryption(dynamic /*SqlConnectionStringBuilder*/ builder)
        {
            // Encryption is required for connections to Azure SQL Database
            builder.Encrypt = true;

            // Always verify server identity
            // builder.TrustServerCertificate defaults to false
        }

        private string ResolveServerFullName()
        {
            // Check if ServerName should be used as ServerFullName verbatim

            if (string.IsNullOrEmpty(ServerName))
                throw new InvalidOperationException("ServerName is required.");

            var shouldUseServerNameVerbatim
                =  ServerName.Contains('.', StringComparison.Ordinal)
                || string.IsNullOrEmpty(ResourceGroupName);

            if (shouldUseServerNameVerbatim)
                return ServerName;

            // Resolve ServerFullName using Az cmdlets

            var value = ScriptBlock
                .Create("param ($x) Get-AzSqlServer @x -ea Stop")
                .Invoke(new Dictionary<string, object>
                {
                    ["ResourceGroupName"] = ResourceGroupName!, // null-checked above
                    ["ServerName"]        = ServerName,
                })
                .FirstOrDefault()
                ?.Properties["FullyQualifiedDomainName"]
                ?.Value as string;

            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    "The Get-AzSqlServer command completed without error, " +
                    "but did not yield an object with a FullyQualifiedDomainName " +
                    "property set to a non-null, non-empty string."
                );

            return ServerFullName = value;
        }
    }
}
