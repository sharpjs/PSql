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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Data.SqlClient;

namespace PSql
{
    /// <summary>
    ///   Information necessary to connect to an Azure SQL Database or
    ///   compatible database.
    /// </summary>
    public class AzureSqlContext : SqlContext
    {
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
        ///   copying property values from the specified instance.
        /// </summary>
        /// <param name="other">
        ///   The instance from which to copy property values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="other"/> is <c>null</c>.
        /// </exception>
        public AzureSqlContext(AzureSqlContext other) : base(other)
        {
            ResourceGroupName  = other.ResourceGroupName;
            ServerFullName     = other.ServerFullName;
            AuthenticationMode = other.AuthenticationMode;
        }

        public sealed override bool IsAzure => true;

        public string ResourceGroupName { get; set; }

        public string ServerFullName { get; internal set; }

        public AzureAuthenticationMode AuthenticationMode { get; set; }

        public new AzureSqlContext Clone() => (AzureSqlContext) CloneCore();

        protected override SqlContext CloneCore() => new AzureSqlContext(this);

        protected override void ConfigureServerName(SqlConnectionStringBuilder builder)
        {
            builder.DataSource = ServerFullName ?? ResolveServerFullName();
        }

        protected override void ConfigureDefaultDatabaseName(SqlConnectionStringBuilder builder)
        {
            if (!string.IsNullOrEmpty(DatabaseName))
                builder.InitialCatalog = DatabaseName;
            else
                builder.InitialCatalog = MasterDatabaseName;
        }

        protected override void ConfigureAuthentication(SqlConnectionStringBuilder builder)
        {
            var auth = (SqlAuthenticationMethod) AuthenticationMode;

            switch (auth)
            {
                case SqlAuthenticationMethod.NotSpecified when Credential != null:
                    auth = SqlAuthenticationMethod.SqlPassword;
                    break;

                case SqlAuthenticationMethod.NotSpecified:
                    auth = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                    break;

                case SqlAuthenticationMethod.SqlPassword:
                case SqlAuthenticationMethod.ActiveDirectoryPassword:
                case SqlAuthenticationMethod.ActiveDirectoryServicePrincipal:
                    if (Credential.IsNullOrEmpty())
                        throw new NotSupportedException("A credential is required when connecting to Azure SQL Database.");
                    break;
            }

            builder.Authentication = auth;
        }

        protected override void ConfigureEncryption(SqlConnectionStringBuilder builder)
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
                    ["ResourceGroupName"] = ResourceGroupName,
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
