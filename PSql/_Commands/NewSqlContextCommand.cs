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
using System.Management.Automation;

// TODO: enable
#nullable disable

namespace PSql
{
    using static ApplicationIntent;

    [Cmdlet(VerbsCommon.New, nameof(SqlContext), DefaultParameterSetName = GenericName)]
    [OutputType(typeof(SqlContext))]
    public class NewSqlContextCommand : PSCmdlet
    {
        private const string
            GenericName = "Generic",
            AzureName   = "Azure",
            CloneName   = "Clone";

        // -Azure
        [Parameter(ParameterSetName = AzureName, Mandatory = true)]
        public SwitchParameter Azure { get; set; }

        // -Source
        [Parameter(ParameterSetName = CloneName, Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNull]
        public SqlContext Source { get; set; }

        // -ResourceGroupName
        [Alias("ResourceGroup")]
        [Parameter(ParameterSetName = AzureName)]
        [Parameter(ParameterSetName = CloneName)]
        public string ResourceGroupName { get; set; }

        // -ServerName
        [Alias("Server")]
        [Parameter(ParameterSetName = GenericName, Position = 0)]
        [Parameter(ParameterSetName = AzureName,   Position = 0, Mandatory = true)]
        [Parameter(ParameterSetName = CloneName,   Position = 0)]
        [AllowNull]
        public string ServerName { get; set; }

        // -DatabaseName
        [Alias("Database")]
        [Parameter(ParameterSetName = GenericName, Position = 1)]
        [Parameter(ParameterSetName = AzureName,   Position = 1, Mandatory = true)]
        [Parameter(ParameterSetName = CloneName,   Position = 1)]
        [AllowNull]
        public string DatabaseName { get; set; }

        // -Credential
        [Parameter(ParameterSetName = GenericName, Position = 2)]
        [Parameter(ParameterSetName = AzureName,   Position = 2)]
        [Parameter(ParameterSetName = CloneName,   Position = 2)]
        [Credential]
        public PSCredential Credential { get; set; } = PSCredential.Empty;

        // -AuthenticationMode
        [Alias("Auth")]
        [Parameter(ParameterSetName = AzureName)]
        [Parameter(ParameterSetName = CloneName)]
        public AzureAuthenticationMode AuthenticationMode { get; set; }

        // -EncryptionMode
        [Alias("Encryption")]
        [Parameter(ParameterSetName = GenericName)]
        [Parameter(ParameterSetName = CloneName)]
        public EncryptionMode EncryptionMode { get; set; }

        // -ServerPort
        [Alias("Port")]
        [Parameter(ParameterSetName = GenericName)]
        [Parameter(ParameterSetName = CloneName)]
        [ValidateRange((ushort) 1, (ushort) 65535)]
        public ushort? ServerPort { get; set; }

        // -InstanceName
        [Alias("Instance")]
        [Parameter(ParameterSetName = GenericName)]
        [Parameter(ParameterSetName = CloneName)]
        //[ValidateNotNullOrEmpty]
        public string InstanceName { get; set; }

        // -ReadOnlyIntent
        [Alias("ReadOnly")]
        [Parameter()]
        public SwitchParameter ReadOnlyIntent { get; set; }

        // -ClientName
        [Alias("Client")]
        [Parameter()]
        [AllowNull]
        public string ClientName { get; set; }

        // -ApplicationName
        [Alias("Application")]
        [Parameter()]
        [AllowNull]
        public string ApplicationName { get; set; }

        // -ConnectTimeout
        [Alias("Timeout")]
        [Parameter()]
        [ValidateRange("0:00:00", "24855.03:14:07")]
        public TimeSpan? ConnectTimeout { get; set; }

        // -ExposeCredentialInConnectionString
        [Parameter()]
        public SwitchParameter ExposeCredentialInConnectionString { get; set; }

        // -Pooling
        [Parameter()]
        public SwitchParameter Pooling { get; set; }

        // -MultipleActiveResultSets
        [Alias("Mars")]
        [Parameter()]
        public SwitchParameter MultipleActiveResultSets { get; set; }

        protected override void ProcessRecord()
        {
            var context = Source?.Clone() ?? CreateContext();

            if (context is AzureSqlContext azureContext)
            {
                if (HasArgument(nameof(ResourceGroupName)))
                    azureContext.ResourceGroupName = ResourceGroupName;

                if (HasArgument(nameof(AuthenticationMode)))
                    azureContext.AuthenticationMode = AuthenticationMode;
            }
            else // (context is not AzureSqlContext)
            {
                if (HasArgument(nameof(EncryptionMode)))
                    context.EncryptionMode = EncryptionMode;
            }

            if (HasArgument(nameof(ServerName)))
                context.ServerName = ServerName;

            if (HasArgument(nameof(ServerPort)))
                context.ServerPort = ServerPort;

            if (HasArgument(nameof(InstanceName)))
                context.InstanceName = InstanceName;

            if (HasArgument(nameof(DatabaseName)))
                context.DatabaseName = DatabaseName;

            if (HasArgument(nameof(Credential)))
                //var credential = Credential.IsNullOrEmpty() ? null : Credential;
                context.Credential = Credential;

            if (HasArgument(nameof(ConnectTimeout)))
                context.ConnectTimeout = ConnectTimeout;

            if (HasArgument(nameof(ClientName)))
                context.ClientName = ClientName;

            if (HasArgument(nameof(ApplicationName)))
                context.ApplicationName = ApplicationName;

            if (HasArgument(nameof(ReadOnlyIntent)))
                context.ApplicationIntent = ReadOnlyIntent ? ReadOnly : ReadWrite;

            if (HasArgument(nameof(ExposeCredentialInConnectionString)))
                context.ExposeCredentialInConnectionString = ExposeCredentialInConnectionString;

            if (HasArgument(nameof(Pooling)))
                context.EnableConnectionPooling = Pooling;

            if (HasArgument(nameof(MultipleActiveResultSets)))
                context.EnableMultipleActiveResultSets = MultipleActiveResultSets;

            WriteObject(context);
        }

        private SqlContext CreateContext()
            => Azure.IsPresent
                ? new AzureSqlContext()
                : new SqlContext();

        private bool HasArgument(string name)
            => MyInvocation.BoundParameters.ContainsKey(name);
    }
}
