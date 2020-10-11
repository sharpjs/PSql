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
        public AzureSqlContext()
        {
            // Encryption is required for connections to Azure SQL Database
            EncryptionMode = EncryptionMode.Full;
        }

        public string ResourceGroupName { get; set; }

        public string ServerFullName { get; private set; }

        public AzureAuthenticationMode AuthenticationMode { get; set; }

        protected override void BuildConnectionString(SqlConnectionStringBuilder builder)
        {
            base.BuildConnectionString(builder);

            builder.DataSource = ServerFullName ?? ResolveServerFullName();

            if (string.IsNullOrEmpty(DatabaseName))
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
