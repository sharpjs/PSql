// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

namespace PSql.Commands;

using static ApplicationIntent;

using AllowNullAttribute = System.Management.Automation.AllowNullAttribute;

/// <summary>
///   The <c>New-SqlContext</c> command.
/// </summary>
[Cmdlet(VerbsCommon.New, nameof(SqlContext), DefaultParameterSetName = GenericName)]
[OutputType(typeof(SqlContext))]
public class NewSqlContextCommand : PSCmdlet
{
    private const string
        GenericName = "Generic",
        AzureName   = "Azure",
        CloneName   = "Clone";

    /// <summary>
    ///   <b>-Azure:</b> TODO
    /// </summary>
    [Parameter(ParameterSetName = AzureName, Mandatory = true)]
    public SwitchParameter Azure { get; set; }

    /// <summary>
    ///   <b>-Source:</b> TODO
    /// </summary>
    [Parameter(ParameterSetName = CloneName, Mandatory = true, ValueFromPipeline = true)]
    [ValidateNotNull]
    public SqlContext? Source { get; set; }

    /// <summary>
    ///   <b>-ResourceGroupName:</b> TODO
    /// </summary>
    [Alias("ResourceGroup", "ServerResourceGroupName")]
    [Parameter(ParameterSetName = AzureName, Position = 0)]
    [Parameter(ParameterSetName = CloneName)]
    [AllowNull, AllowEmptyString]
    public string? ResourceGroupName { get; set; }

    /// <summary>
    ///   <b>-ServerResourceName:</b> TODO
    /// </summary>
    [Alias("Resource")]
    [Parameter(ParameterSetName = AzureName,  Position = 1)]
    [Parameter(ParameterSetName = CloneName)]
    [AllowNull, AllowEmptyString]
    public string? ServerResourceName { get; set; }

    /// <summary>
    ///   <b>-ServerName:</b> TODO
    /// </summary>
    [Alias("Server")]
    [Parameter(ParameterSetName = GenericName, Position = 0)]
    [Parameter(ParameterSetName = AzureName)]
    [Parameter(ParameterSetName = CloneName,   Position = 0)]
    [AllowNull, AllowEmptyString]
    public string? ServerName { get; set; }

    /// <summary>
    ///   <b>-DatabaseName:</b> TODO
    /// </summary>
    [Alias("Database")]
    [Parameter(ParameterSetName = GenericName, Position = 1)]
    [Parameter(ParameterSetName = AzureName,   Position = 2)]
    [Parameter(ParameterSetName = CloneName,   Position = 1)]
    [AllowNull, AllowEmptyString]
    public string? DatabaseName { get; set; }

    /// <summary>
    ///   <b>-AuthenticationMode:</b> TODO
    /// </summary>
    [Alias("Auth")]
    [Parameter(ParameterSetName = AzureName)]
    [Parameter(ParameterSetName = CloneName)]
    public AzureAuthenticationMode AuthenticationMode { get; set; }

    /// <summary>
    ///   <b>-Credential:</b> TODO
    /// </summary>
    [Parameter]
    [Credential]
    [AllowNull]
    public PSCredential? Credential { get; set; } = PSCredential.Empty;

    /// <summary>
    ///   <b>-EncryptionMode:</b> TODO
    /// </summary>
    [Alias("Encryption")]
    [Parameter(ParameterSetName = GenericName)]
    [Parameter(ParameterSetName = CloneName)]
    public EncryptionMode EncryptionMode { get; set; }

    /// <summary>
    ///   <b>-ServerPort:</b> TODO
    /// </summary>
    [Alias("Port")]
    [Parameter(ParameterSetName = GenericName)]
    [Parameter(ParameterSetName = CloneName)]
    [ValidateNullOrPositiveUInt16]
    public ushort? ServerPort { get; set; }

    /// <summary>
    ///   <b>-InstanceName:</b> TODO
    /// </summary>
    [Alias("Instance")]
    [Parameter(ParameterSetName = GenericName)]
    [Parameter(ParameterSetName = CloneName)]
    [AllowNull, AllowEmptyString]
    public string? InstanceName { get; set; }

    /// <summary>
    ///   <b>-ReadOnlyIntent:</b> TODO
    /// </summary>
    [Alias("ReadOnly")]
    [Parameter]
    public SwitchParameter ReadOnlyIntent { get; set; }

    /// <summary>
    ///   <b>-ClientName:</b> TODO
    /// </summary>
    [Alias("Client")]
    [Parameter]
    [AllowNull, AllowEmptyString]
    public string? ClientName { get; set; }

    /// <summary>
    ///   <b>-ApplicationName:</b> TODO
    /// </summary>
    [Alias("Application")]
    [Parameter]
    [AllowNull, AllowEmptyString]
    public string? ApplicationName { get; set; }

    /// <summary>
    ///   <b>-ConnectTimeout:</b> TODO
    /// </summary>
    [Alias("Timeout")]
    [Parameter]
    [ValidateNullOrTimeout]
    public TimeSpan? ConnectTimeout { get; set; }

    /// <summary>
    ///   <b>-ExposeCredentialInConnectionString:</b> TODO
    /// </summary>
    [Parameter]
    public SwitchParameter ExposeCredentialInConnectionString { get; set; }

    /// <summary>
    ///   <b>-Pooling:</b> TODO
    /// </summary>
    [Parameter]
    public SwitchParameter Pooling { get; set; }

    /// <summary>
    ///   <b>-MultipleActiveResultSets:</b> TODO
    /// </summary>
    [Alias("Mars")]
    [Parameter]
    public SwitchParameter MultipleActiveResultSets { get; set; }

    /// <summary>
    ///   <b>-Frozen:</b> TODO
    /// </summary>
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
