// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Linq.Expressions;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace PSql.Tests.Unit;

using static AzureAuthenticationMode;
using static SqlClientVersion;

using Case = TestCaseData;

[TestFixture]
public class AzureSqlContextTests
{
    private const string
        Auth_AadPassword         = @"Authentication=""Active Directory Password"";",
        Auth_AadIntegrated       = @"Authentication=""Active Directory Integrated"";",
        Auth_AadInteractive      = @"Authentication=""Active Directory Interactive"";",
        Auth_AadServicePrincipal = @"Authentication=""Active Directory Service Principal"";",
        Auth_AadDeviceCodeFlow   = @"Authentication=""Active Directory Device Code Flow"";",
        Auth_AadManagedIdentity  = @"Authentication=""Active Directory Managed Identity"";",
        Auth_AadDefault          = @"Authentication=""Active Directory Default"";",
        UserName                 = "User ID=user;",
        Password                 = "Password=pass;";

    [Test]
    public void Defaults()
    {
        var context = new AzureSqlContext();

        context.IsAzure                            .ShouldBeTrue();
        context.AsAzure                            .ShouldBeSameAs(context);
        context.IsLocal                            .ShouldBeFalse();
        context.IsFrozen                           .ShouldBeFalse();

        context.ServerResourceGroupName            .ShouldBeNull();
        context.ServerResourceName                 .ShouldBeNull();
        context.ServerName                         .ShouldBeNull();
        context.ServerPort                         .ShouldBeNull();
        context.InstanceName                       .ShouldBeNull();
        context.DatabaseName                       .ShouldBeNull();
        context.AuthenticationMode                 .ShouldBe(Default);
        context.Credential                         .ShouldBeNull();
        context.EncryptionMode                     .ShouldBe(EncryptionMode.Full); // different from SqlContext
        context.ConnectTimeout                     .ShouldBeNull();
        context.ClientName                         .ShouldBeNull();
        context.ApplicationName                    .ShouldBeNull();
        context.ApplicationIntent                  .ShouldBe(ApplicationIntent.ReadWrite);
        context.ExposeCredentialInConnectionString .ShouldBeFalse();
        context.EnableConnectionPooling            .ShouldBeFalse();
        context.EnableMultipleActiveResultSets     .ShouldBeFalse();
    }

    private AzureSqlContext MakeExampleContext(bool frozen = false)
    {
        var credential = new PSCredential("username", "password".Secure());

        var context = new AzureSqlContext
        {
            ServerResourceGroupName            = "resource-group",
            ServerResourceName                 = "server",
            ServerName                         = "server.example.com",
            ServerPort                         = 1234,
            InstanceName                       = "instance",
            DatabaseName                       = "database",
            AuthenticationMode                 = SqlPassword,
            Credential                         = credential,
            ConnectTimeout                     = 42.Seconds(),
            ClientName                         = "client",
            ApplicationName                    = "application",
            ApplicationIntent                  = ApplicationIntent.ReadOnly,
            ExposeCredentialInConnectionString = true,
            EnableConnectionPooling            = true,
            EnableMultipleActiveResultSets     = true,
        };

        if (frozen) context.Freeze();

        return context;
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void Clone_Constructor(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = new AzureSqlContext(original);

        ShouldBeClone(clone, original);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void Clone_Concrete(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original.Clone();

        ShouldBeClone(clone, original);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void Clone_Abstract(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = ((ICloneable) original).Clone();

        ShouldBeClone(clone, original);
    }

    private static void ShouldBeClone(object? obj, AzureSqlContext original)
    {
        var clone = obj.ShouldBeOfType<AzureSqlContext>();

        clone.ShouldNotBeSameAs(original);

        // Invariants
        clone.IsAzure                            .ShouldBeTrue();
        clone.AsAzure                            .ShouldBeSameAs(clone);
        clone.IsLocal                            .ShouldBeFalse();
        clone.IsFrozen                           .ShouldBeFalse(); // diff behavior from indexer

        // Cloned properties
        clone.ServerResourceGroupName            .ShouldBe(original.ServerResourceGroupName);
        clone.ServerResourceName                 .ShouldBe(original.ServerResourceName);
        clone.ServerName                         .ShouldBe(original.ServerName);
        clone.ServerPort                         .ShouldBe(original.ServerPort);
        clone.InstanceName                       .ShouldBe(original.InstanceName);
        clone.DatabaseName                       .ShouldBe(original.DatabaseName);
        clone.AuthenticationMode                 .ShouldBe(original.AuthenticationMode);
        clone.Credential                         .ShouldBe(original.Credential);
        clone.EncryptionMode                     .ShouldBe(original.EncryptionMode);
        clone.ConnectTimeout                     .ShouldBe(original.ConnectTimeout);
        clone.ClientName                         .ShouldBe(original.ClientName);
        clone.ApplicationName                    .ShouldBe(original.ApplicationName);
        clone.ApplicationIntent                  .ShouldBe(original.ApplicationIntent);
        clone.ExposeCredentialInConnectionString .ShouldBe(original.ExposeCredentialInConnectionString);
        clone.EnableConnectionPooling            .ShouldBe(original.EnableConnectionPooling);
        clone.EnableMultipleActiveResultSets     .ShouldBe(original.EnableMultipleActiveResultSets);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void Indexer_ResourceGroupName_ServerName_DatabaseName(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original["rg2", "srv2", "db2"];

        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(original);

        // Invariants
        clone.IsAzure                            .ShouldBeTrue();
        clone.AsAzure                            .ShouldBeSameAs(clone);
        clone.IsLocal                            .ShouldBeFalse();

        // Parameterized properties
        clone.ServerResourceGroupName            .ShouldBe("rg2");
        clone.ServerResourceName                 .ShouldBe("srv2");
        clone.DatabaseName                       .ShouldBe("db2");

        // Cloned properties
        clone.IsFrozen                           .ShouldBe(original.IsFrozen); // diff behavior from Clone()
        clone.ServerName                         .ShouldBe(original.ServerName); // unused by Azure context
        clone.ServerPort                         .ShouldBe(original.ServerPort);
        clone.InstanceName                       .ShouldBe(original.InstanceName);
        clone.AuthenticationMode                 .ShouldBe(original.AuthenticationMode);
        clone.Credential                         .ShouldBe(original.Credential);
        clone.EncryptionMode                     .ShouldBe(original.EncryptionMode);
        clone.ConnectTimeout                     .ShouldBe(original.ConnectTimeout);
        clone.ClientName                         .ShouldBe(original.ClientName);
        clone.ApplicationName                    .ShouldBe(original.ApplicationName);
        clone.ApplicationIntent                  .ShouldBe(original.ApplicationIntent);
        clone.ExposeCredentialInConnectionString .ShouldBe(original.ExposeCredentialInConnectionString);
        clone.EnableConnectionPooling            .ShouldBe(original.EnableConnectionPooling);
        clone.EnableMultipleActiveResultSets     .ShouldBe(original.EnableMultipleActiveResultSets);
    }

    public static readonly IEnumerable<Case> PropertyCases =
    [
        PropertyCase(c => c.ServerResourceGroupName, "resource-group"),
        PropertyCase(c => c.ServerResourceName,      "server"),
        PropertyCase(c => c.AuthenticationMode,      AadDeviceCodeFlow),
    ];

    public static Case PropertyCase<T>(Expression<Func<AzureSqlContext, T>> property, T value)
    {
        var memberExpression = (MemberExpression) property.Body;
        var propertyInfo     = (PropertyInfo)     memberExpression.Member;

        return new Case(propertyInfo, value);
    }

    [Test]
    [TestCaseSource(nameof(PropertyCases))]
    public void Property_Set_NotFrozen(PropertyInfo property, object? value)
    {
        var context = new AzureSqlContext();

        property.SetValue(context, value);

        property.GetValue(context).ShouldBe(value);
    }

    [Test]
    [TestCaseSource(nameof(PropertyCases))]
    public void Property_Set_Frozen(PropertyInfo property, object? value)
    {
        var context = new AzureSqlContext();

        context.Freeze();

        Should.Throw<TargetInvocationException>( // due to reflection
            () => property.SetValue(context, value)
        )
        .InnerException.ShouldBeOfType<InvalidOperationException>()
        .Message.ShouldStartWith("The context is frozen and cannot be modified.");
    }

    [Test]
    public void IsLocal_Get()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = "rg",
            ServerResourceName      = "srv",
        };

        context.IsLocal.ShouldBeFalse();
    }

    [Test]
    public void EncryptionMode_Set()
    {
        var context = new AzureSqlContext();

        context.EncryptionMode = EncryptionMode.None; // Ignored in Azure context

        context.EncryptionMode.ShouldBe(EncryptionMode.Full);
    }

    [Test]
    [TestCase(null,                           "resolved-server.example.com")]
    [TestCase("explicit-server.example.com",  "explicit-server.example.com")]
    public void GetEffectiveServerName(string? serverName, string expected)
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            ServerName              = serverName
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "resolved-server.example.com" }
        );

        context.GetEffectiveServerName().ShouldBe(expected);
        context.GetEffectiveServerName().ShouldBeSameAs(
            context.GetEffectiveServerName(),
            "the method should cache its return value"
        );
    }

    [Test]
    public void GetEffectiveServerName_NoServerResourceGroupName()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = null,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName
        };

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetEffectiveServerName_NoServerResourceName()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = null
        };

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetEffectiveServerName_GetAzSqlServerNoOutput()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName
        };

        using var _ = WithGetAzSqlServerOutput();

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetEffectiveServerName_GetAzSqlServerNoFqdnProperty()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName
        };

        using var _ = WithGetAzSqlServerOutput(
            new { Description = "foo.example.com" }
        );

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetEffectiveServerName_GetAzSqlServerFqdnNotString()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = new object() } // not a string
        );

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetEffectiveServerName_GetAzSqlServerFqdnEmpty()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "" }
        );

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetConnectionString_ExplicitDatabase_Property()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            DatabaseName            = "bar",
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        context.GetConnectionString(databaseName: null, Latest).ShouldBe(
            @"Data Source=foo.example.com;Initial Catalog=bar;" +
            @"Authentication=""Active Directory Integrated"";Encrypt=true;Pooling=false"
        );
    }

    [Test]
    public void GetConnectionString_ExplicitDatabase_Parameter()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            DatabaseName            = "bar",
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        context.GetConnectionString(databaseName: "quux", Latest).ShouldBe(
            @"Data Source=foo.example.com;Initial Catalog=quux;" +
            @"Authentication=""Active Directory Integrated"";Encrypt=true;Pooling=false"
        );
    }

    [Test]
    [TestCase(Default,            Auth_AadIntegrated)]
    [TestCase(AadIntegrated,      Auth_AadIntegrated)]
    [TestCase(AadInteractive,     Auth_AadInteractive)]
    [TestCase(AadDeviceCodeFlow,  Auth_AadDeviceCodeFlow)]
    [TestCase(AadManagedIdentity, Auth_AadManagedIdentity)] // credential is optional
    [TestCase(AadDefault,         Auth_AadDefault)]         // credential is optional
    public void GetConnectionString_NoCredential(AzureAuthenticationMode mode, string fragment)
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            AuthenticationMode      = mode,
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        context.GetConnectionString(databaseName: null, Latest).ShouldBe(
            $@"Data Source=foo.example.com;Initial Catalog=master;{
               fragment}Encrypt=true;Pooling=false"
        );
    }

    [Test]
    [TestCase(Default            )] // when no credential
    [TestCase(AadPassword        )] // support check happens before credential check
    [TestCase(AadIntegrated      )]
    [TestCase(AadInteractive     )]
    [TestCase(AadServicePrincipal)] // support check happens before credential check
    [TestCase(AadDeviceCodeFlow  )]
    [TestCase(AadManagedIdentity )] // credential is optional
    [TestCase(AadDefault         )] // credential is optional
    public void GetConnectionString_NoCredential_Unsupported(AzureAuthenticationMode mode)
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            AuthenticationMode      = mode,
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        Should.Throw<NotSupportedException>(
            () => context.GetConnectionString(databaseName: null, Legacy)
        );
    }

    [Test]
    [TestCase(SqlPassword        )]
    [TestCase(AadPassword        )]
    [TestCase(AadServicePrincipal)]
    public void GetConnectionString_NoCredential_Required(AzureAuthenticationMode mode)
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            AuthenticationMode      = mode,
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        Should.Throw<NotSupportedException>(
            () => context.GetConnectionString(databaseName: null, Latest)
        );
    }

    [Test]
    [TestCase(Default,                                         UserName + Password)]
    [TestCase(SqlPassword,                                     UserName + Password)]
    [TestCase(AadPassword,          Auth_AadPassword         + UserName + Password)]
    [TestCase(AadIntegrated,        Auth_AadIntegrated                            )]
    [TestCase(AadInteractive,       Auth_AadInteractive                           )]
    [TestCase(AadServicePrincipal,  Auth_AadServicePrincipal + UserName + Password)]
    [TestCase(AadDeviceCodeFlow,    Auth_AadDeviceCodeFlow                        )]
    [TestCase(AadManagedIdentity,   Auth_AadManagedIdentity  + UserName           )]
    [TestCase(AadDefault,           Auth_AadDefault          + UserName           )]
    public void GetConnectionString_ExplicitCredential(AzureAuthenticationMode mode, string fragment)
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            AuthenticationMode      = mode,
            Credential              = new("user", "pass".Secure()),
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        context.GetConnectionString(databaseName: null, Latest).ShouldBe(
            $@"Data Source=foo.example.com;Initial Catalog=master;{
               fragment}Encrypt=true;Pooling=false"
        );
    }

    [Test]
    public void GetConnectionString_ExplicitCredential_Omit()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = FakeGetAzSqlServerCommand.ExpectedServerName,
            Credential              = new("user", "pass".Secure()),
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        context.GetConnectionString(databaseName: null, Latest, omitCredential: true).ShouldBe(
            @"Data Source=foo.example.com;Initial Catalog=master;" +
            @"Encrypt=true;Pooling=false"
        );
    }

    [Test]
    public void GetConnectionString_ExplicitCredential_Expose()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName            = FakeGetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName                 = FakeGetAzSqlServerCommand.ExpectedServerName,
            Credential                         = new("user", "pass".Secure()),
            ExposeCredentialInConnectionString = true,
        };

        using var _ = WithGetAzSqlServerOutput(
            new { FullyQualifiedDomainName = "foo.example.com" }
        );

        // NOTE: Ignored because ExposeCredentialInConnectionString takes precedence
        //                                                    vvvvvvvvvvvvvvvvvvvv
        context.GetConnectionString(databaseName: null, Latest, omitCredential: true).ShouldBe(
            @"Data Source=foo.example.com;Initial Catalog=master;" +
            @"User ID=user;Password=pass;Persist Security Info=true;Encrypt=true;Pooling=false"
        );
    }

    private static RunspaceScope WithGetAzSqlServerOutput(params object?[] output)
    {
        var state = InitialSessionState.CreateDefault();

        state.Variables.Add(new SessionStateVariableEntry(
            FakeGetAzSqlServerCommand.ResultVariableName,
            value:       output,
            description: null
        ));

        state.Commands.Add(new SessionStateCmdletEntry(
            "Get-AzSqlServer",
            typeof(FakeGetAzSqlServerCommand),
            helpFileName: null
        ));

        return new(state);
    }

    [Cmdlet(VerbsCommon.Get, "AzSqlServer")]
    [OutputType(typeof(PSObject))]
    private class FakeGetAzSqlServerCommand : PSCmdlet
    {
        public const string
            ExpectedResourceGroupName = "resource-group",
            ExpectedServerName        = "server",
            ResultVariableName        = "FakeAzSqlServer";

        [Parameter(Mandatory = true)]
        public string? ResourceGroupName { get; set; }

        [Parameter(Mandatory = true)]
        public string? ServerName { get; set; }

        protected override void ProcessRecord()
        {
            ResourceGroupName.ShouldBe(ExpectedResourceGroupName);
            ServerName       .ShouldBe(ExpectedServerName);

            WriteObject(
                SessionState.PSVariable.GetValue(ResultVariableName),
                enumerateCollection: true
            );
        }
    }
}
