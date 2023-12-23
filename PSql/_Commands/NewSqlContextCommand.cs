// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql;

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
    public SqlContext? Source { get; set; }

    // -ResourceGroupName
    [Alias("ResourceGroup", "ServerResourceGroupName")]
    [Parameter(ParameterSetName = AzureName, Position = 0)]
    [Parameter(ParameterSetName = CloneName)]
    [AllowNull, AllowEmptyString]
    public string? ResourceGroupName { get; set; }

    // -ServerResourceName
    [Alias("Resource")]
    [Parameter(ParameterSetName = AzureName,  Position = 1)]
    [Parameter(ParameterSetName = CloneName)]
    [AllowNull, AllowEmptyString]
    public string? ServerResourceName { get; set; }

    // -ServerName
    [Alias("Server")]
    [Parameter(ParameterSetName = GenericName, Position = 0)]
    [Parameter(ParameterSetName = AzureName)]
    [Parameter(ParameterSetName = CloneName,   Position = 0)]
    [AllowNull, AllowEmptyString]
    public string? ServerName { get; set; }

    // -DatabaseName
    [Alias("Database")]
    [Parameter(ParameterSetName = GenericName, Position = 1)]
    [Parameter(ParameterSetName = AzureName,   Position = 2)]
    [Parameter(ParameterSetName = CloneName,   Position = 1)]
    [AllowNull, AllowEmptyString]
    public string? DatabaseName { get; set; }

    // -AuthenticationMode
    [Alias("Auth")]
    [Parameter(ParameterSetName = AzureName)]
    [Parameter(ParameterSetName = CloneName)]
    public AzureAuthenticationMode AuthenticationMode { get; set; }

    // -Credential
    [Parameter]
    [Credential]
    [AllowNull]
    public PSCredential? Credential { get; set; } = PSCredential.Empty;

    // -EncryptionMode
    [Alias("Encryption")]
    [Parameter(ParameterSetName = GenericName)]
    [Parameter(ParameterSetName = CloneName)]
    public EncryptionMode EncryptionMode { get; set; }

    // -ServerPort
    [Alias("Port")]
    [Parameter(ParameterSetName = GenericName)]
    [Parameter(ParameterSetName = CloneName)]
    [ValidateNullOrPositiveUInt16]
    public ushort? ServerPort { get; set; }

    // -InstanceName
    [Alias("Instance")]
    [Parameter(ParameterSetName = GenericName)]
    [Parameter(ParameterSetName = CloneName)]
    [AllowNull, AllowEmptyString]
    public string? InstanceName { get; set; }

    // -ReadOnlyIntent
    [Alias("ReadOnly")]
    [Parameter]
    public SwitchParameter ReadOnlyIntent { get; set; }

    // -ClientName
    [Alias("Client")]
    [Parameter]
    [AllowNull, AllowEmptyString]
    public string? ClientName { get; set; }

    // -ApplicationName
    [Alias("Application")]
    [Parameter]
    [AllowNull, AllowEmptyString]
    public string? ApplicationName { get; set; }

    // -ConnectTimeout
    [Alias("Timeout")]
    [Parameter]
    [ValidateNullOrTimeout]
    public TimeSpan? ConnectTimeout { get; set; }

    // -ExposeCredentialInConnectionString
    [Parameter]
    public SwitchParameter ExposeCredentialInConnectionString { get; set; }

    // -Pooling
    [Parameter]
    public SwitchParameter Pooling { get; set; }

    // -MultipleActiveResultSets
    [Alias("Mars")]
    [Parameter]
    public SwitchParameter MultipleActiveResultSets { get; set; }

    // -Frozen
    [Parameter]
    public SwitchParameter Frozen { get; set; }

    protected override void ProcessRecord()
    {
        var context = Source?.Clone() ?? CreateContext();

        foreach (var parameterName in MyInvocation.BoundParameters.Keys)
            ApplyParameterValue(context, parameterName);

        if (Frozen)
            context.Freeze();

        WriteObject(context);
    }

    private void ApplyParameterValue(SqlContext context, string parameterName)
    {
        switch (parameterName)
        {
            case nameof(ResourceGroupName):                  SetServerResourceGroupName            (context); break;
            case nameof(ServerResourceName):                 SetServerResourceName                 (context); break;
            case nameof(ServerName):                         SetServerName                         (context); break;
            case nameof(ServerPort):                         SetServerPort                         (context); break;
            case nameof(InstanceName):                       SetInstanceName                       (context); break;
            case nameof(DatabaseName):                       SetDatabaseName                       (context); break;
            case nameof(AuthenticationMode):                 SetAuthenticationMode                 (context); break;
            case nameof(Credential):                         SetCredential                         (context); break;
            case nameof(EncryptionMode):                     SetEncryptionMode                     (context); break;
            case nameof(ConnectTimeout):                     SetConnectTimeout                     (context); break;
            case nameof(ClientName):                         SetClientName                         (context); break;
            case nameof(ApplicationName):                    SetApplicationName                    (context); break;
            case nameof(ReadOnlyIntent):                     SetApplicationIntent                  (context); break;
            case nameof(ExposeCredentialInConnectionString): SetExposeCredentialInConnectionString (context); break;
            case nameof(Pooling):                            SetEnableConnectionPooling            (context); break;
            case nameof(MultipleActiveResultSets):           SetEnableMultipleActiveResultSets     (context); break;
        }
    }

    private SqlContext CreateContext()
    {
        return Azure.IsPresent
            ? new AzureSqlContext()
            : new SqlContext();
    }

    private void SetServerResourceGroupName(SqlContext context)
    {
        if (context is AzureSqlContext azureContext)
            azureContext.ServerResourceGroupName = ResourceGroupName.NullIfEmpty();
        else
            WarnIgnoredBecauseNotAzureContext(nameof(ResourceGroupName));
    }

    private void SetServerResourceName(SqlContext context)
    {
        if (context is AzureSqlContext azureContext)
            azureContext.ServerResourceName = ServerResourceName.NullIfEmpty();
        else
            WarnIgnoredBecauseNotAzureContext(nameof(ServerResourceName));
    }

    private void SetServerName(SqlContext context)
    {
        context.ServerName = ServerName.NullIfEmpty();
    }

    private void SetServerPort(SqlContext context)
    {
        context.ServerPort = ServerPort;
    }

    private void SetInstanceName(SqlContext context)
    {
        context.InstanceName = InstanceName.NullIfEmpty();
    }

    private void SetDatabaseName(SqlContext context)
    {
        context.DatabaseName = DatabaseName.NullIfEmpty();
    }

    private void SetAuthenticationMode(SqlContext context)
    {
        if (context is AzureSqlContext azureContext)
            azureContext.AuthenticationMode = AuthenticationMode;
        else
            WarnIgnoredBecauseNotAzureContext(nameof(AuthenticationMode));
    }

    private void SetCredential(SqlContext context)
    {
        context.Credential = Credential.NullIfEmpty();
    }

    private void SetEncryptionMode(SqlContext context)
    {
        if (context is AzureSqlContext)
            WarnIgnoredBecauseAzureContext(nameof(EncryptionMode));
        else
            context.EncryptionMode = EncryptionMode;
    }

    private void SetConnectTimeout(SqlContext context)
    {
        context.ConnectTimeout = ConnectTimeout;
    }

    private void SetClientName(SqlContext context)
    {
        context.ClientName = ClientName.NullIfEmpty();
    }

    private void SetApplicationName(SqlContext context)
    {
        context.ApplicationName = ApplicationName.NullIfEmpty();
    }

    private void SetApplicationIntent(SqlContext context)
    {
        context.ApplicationIntent = ReadOnlyIntent ? ReadOnly : ReadWrite;
    }

    private void SetExposeCredentialInConnectionString(SqlContext context)
    {
        context.ExposeCredentialInConnectionString = ExposeCredentialInConnectionString;
    }

    private void SetEnableConnectionPooling(SqlContext context)
    {
        context.EnableConnectionPooling = Pooling;
    }

    private void SetEnableMultipleActiveResultSets(SqlContext context)
    {
        context.EnableMultipleActiveResultSets = MultipleActiveResultSets;
    }

    private void WarnIgnoredBecauseAzureContext(string parameterName)
    {
        WriteWarning(string.Format(
            "The '{0}' argument was ignored because " +
            "the context is an Azure SQL Database context.",
            parameterName
        ));
    }

    private void WarnIgnoredBecauseNotAzureContext(string parameterName)
    {
        WriteWarning(string.Format(
            "The '{0}' argument was ignored because " +
            "the context is not an Azure SQL Database context.",
            parameterName
        ));
    }
}
