// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Linq.Expressions;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace PSql.Tests.Unit;

using Case = TestCaseData;

[TestFixture]
public class AzureSqlContextTests
{
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
        context.AuthenticationMode                 .ShouldBe(AzureAuthenticationMode.Default);
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
            AuthenticationMode                 = AzureAuthenticationMode.SqlPassword,
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
    [TestCase(true)]
    public void Clone_Typed(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original.Clone();

        clone.ShouldNotBeNull();
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
    [TestCase(true)]
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
        PropertyCase(c => c.AuthenticationMode,      AzureAuthenticationMode.AadDeviceCodeFlow),
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
    [TestCase(null,                           "resolved-server.example.com")]
    [TestCase("explicit-server.example.com",  "explicit-server.example.com")]
    public void GetEffectiveServerName(string? serverName, string expected)
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = GetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = GetAzSqlServerCommand.ExpectedServerName,
            ServerName              = serverName
        };

        var state = InitialSessionState.CreateDefault();

        state.Variables.Add(new SessionStateVariableEntry(
            GetAzSqlServerCommand.ResultVariableName,
            new { FullyQualifiedDomainName = "resolved-server.example.com" },
            description: null
        ));

        state.Commands.Add(new SessionStateCmdletEntry(
            "Get-AzSqlServer",
            typeof(GetAzSqlServerCommand),
            helpFileName: null
        ));

        using var _ = new RunspaceScope(state);

        context.GetEffectiveServerName().ShouldBe(expected);
    }

    [Test]
    public void GetEffectiveServerName_NoServerResourceGroupName()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = null,
            ServerResourceName      = GetAzSqlServerCommand.ExpectedServerName
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
            ServerResourceGroupName = GetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = null
        };

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Test]
    public void GetEffectiveServerName_UnexpectAzSqlServerObject()
    {
        var context = new AzureSqlContext
        {
            ServerResourceGroupName = GetAzSqlServerCommand.ExpectedResourceGroupName,
            ServerResourceName      = GetAzSqlServerCommand.ExpectedServerName
        };

        var state = InitialSessionState.CreateDefault();

        state.Variables.Add(new SessionStateVariableEntry(
            GetAzSqlServerCommand.ResultVariableName,
            new { Description = "something unexpected" },
            description: null
        ));

        state.Commands.Add(new SessionStateCmdletEntry(
            "Get-AzSqlServer",
            typeof(GetAzSqlServerCommand),
            helpFileName: null
        ));

        using var _ = new RunspaceScope(state);

        Should.Throw<InvalidOperationException>(
            () => context.GetEffectiveServerName()
        );
    }

    [Cmdlet(VerbsCommon.Get, "AzSqlServer")]
    [OutputType(typeof(PSObject))]
    private class GetAzSqlServerCommand : PSCmdlet
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

            WriteObject(SessionState.PSVariable.GetValue(ResultVariableName));
        }
    }
}
