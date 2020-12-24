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
using System.Management.Automation;

// TODO: enable
#nullable disable

namespace PSql
{
    using static ApplicationIntent;

    [Cmdlet(VerbsCommon.New, nameof(SqlContext), DefaultParameterSetName = GenericName)]
    [OutputType(typeof(SqlContext))]
    public class NewSqlContextCommand : Cmdlet
    {
        private const string
            GenericName = "Generic",
            AzureName   = "Azure";

        // -Azure
        [Parameter(ParameterSetName = AzureName, Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Azure { get; set; }

        // -ResourceGroupName
        [Alias("ResourceGroup")]
        [Parameter(ParameterSetName = AzureName, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        // -ServerName
        [Alias("Server")]
        [Parameter(ParameterSetName = GenericName, Position = 0,                   ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServerName { get; set; }

        // -DatabaseName
        [Alias("Database")]
        [Parameter(ParameterSetName = GenericName, Position = 1,                   ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName { get; set; }

        // -Credential
        [Parameter(ParameterSetName = GenericName, Position = 2, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = AzureName,   Position = 3, ValueFromPipelineByPropertyName = true)]
        [Credential]
        public PSCredential Credential { get; set; } = PSCredential.Empty;

        // -AuthenticationMode
        [Alias("Auth")]
        [Parameter(ParameterSetName = AzureName, ValueFromPipelineByPropertyName = true)]
        public AzureAuthenticationMode AuthenticationMode { get; set; }

        // -EncryptionMode
        [Alias("Encryption")]
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        public EncryptionMode EncryptionMode { get; set; }

        // -ServerPort
        [Alias("Port")]
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        [ValidateRange((ushort) 1, (ushort) 65535)]
        public ushort? ServerPort { get; set; }

        // -InstanceName
        [Alias("Instance")]
        [Parameter(ParameterSetName = GenericName, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string InstanceName { get; set; }

        // -ReadOnlyIntent
        [Alias("ReadOnly")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter ReadOnlyIntent { get; set; }

        // -ClientName
        [Alias("Client")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ClientName { get; set; }

        // -ApplicationName
        [Alias("Application")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ApplicationName { get; set; }

        // -ConnectTimeout
        [Alias("Timeout")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateRange("0:00:00", "24855.03:14:07")]
        public TimeSpan? ConnectTimeout { get; set; }

        // -ExposeCredentialInConnectionString
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter ExposeCredentialInConnectionString { get; set; }

        // -Pooling
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Pooling { get; set; }

        // -MultipleActiveResultSets
        [Alias("Mars")]
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter MultipleActiveResultSets { get; set; } = true;

        protected override void ProcessRecord()
        {
            var context = Azure.IsPresent
                ? new AzureSqlContext { ResourceGroupName  = ResourceGroupName  ,
                                        AuthenticationMode = AuthenticationMode }
                : new SqlContext      { EncryptionMode     = EncryptionMode     };

            var credential = Credential.IsNullOrEmpty()
                ? null
                : Credential;

            context.ServerName        = ServerName;
            context.ServerPort        = ServerPort;
            context.InstanceName      = InstanceName;
            context.DatabaseName      = DatabaseName;
            context.Credential        = credential;
            context.ConnectTimeout    = ConnectTimeout;
            context.ClientName        = ClientName;
            context.ApplicationName   = ApplicationName;
            context.ApplicationIntent = ReadOnlyIntent ? ReadOnly : ReadWrite;

            context.ExposeCredentialInConnectionString = ExposeCredentialInConnectionString;
            context.EnableConnectionPooling            = Pooling;
            context.EnableMultipleActiveResultSets     = MultipleActiveResultSets;

            WriteObject(context);
        }
    }
}
