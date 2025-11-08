// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Linq.Expressions;
using System.Reflection;

namespace PSql.Tests.Unit;

using Case = TestCaseData;

[TestFixture]
public class SqlContextTests
{
    [Test]
    public void Defaults()
    {
        var context = new SqlContext();

        context.IsAzure                            .ShouldBeFalse();
        context.AsAzure                            .ShouldBeNull();
        context.IsLocal                            .ShouldBeTrue();
        context.IsFrozen                           .ShouldBeFalse();

        context.ServerName                         .ShouldBeNull();
        context.ServerPort                         .ShouldBeNull();
        context.InstanceName                       .ShouldBeNull();
        context.DatabaseName                       .ShouldBeNull();
        context.Credential                         .ShouldBeNull();
        context.EncryptionMode                     .ShouldBe(EncryptionMode.Default);
        context.ConnectTimeout                     .ShouldBeNull();
        context.ClientName                         .ShouldBeNull();
        context.ApplicationName                    .ShouldBeNull();
        context.ApplicationIntent                  .ShouldBe(ApplicationIntent.ReadWrite);
        context.ExposeCredentialInConnectionString .ShouldBeFalse();
        context.EnableConnectionPooling            .ShouldBeFalse();
        context.EnableMultipleActiveResultSets     .ShouldBeFalse();
    }

    public void Freeze()
    {
        var context = new SqlContext();

        var frozen = context.Freeze();

        frozen         .ShouldBeSameAs(context);
        frozen.IsFrozen.ShouldBeTrue();
    }

    public static SqlContext MakeExampleContext(bool frozen = false)
    {
        var credential = new PSCredential("username", "password".Secure());

        var context = new SqlContext
        {
            ServerName                         = "server",
            ServerPort                         = 1234,
            InstanceName                       = "instance",
            DatabaseName                       = "database",
            Credential                         = credential,
            EncryptionMode                     = EncryptionMode.Full,
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
        clone.IsAzure                            .ShouldBeFalse();
        clone.AsAzure                            .ShouldBeNull();
        clone.IsFrozen                           .ShouldBeFalse(); // diff behavior from indexer

        // Cloned properties
        clone.IsLocal                            .ShouldBe(original.IsLocal);
        clone.ServerName                         .ShouldBe(original.ServerName);
        clone.ServerPort                         .ShouldBe(original.ServerPort);
        clone.InstanceName                       .ShouldBe(original.InstanceName);
        clone.DatabaseName                       .ShouldBe(original.DatabaseName);
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
    public void Indexer_ScriptBlock(bool frozen)
    {
        using var _  = new RunspaceScope();
        var original = MakeExampleContext(frozen);

        var clone = original[ScriptBlock.Create("$_.ServerPort = 42")];

        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(original);

        // Invariants
        clone.IsAzure                            .ShouldBeFalse();
        clone.AsAzure                            .ShouldBeNull();

        // Properties modified by script block
        ((int?) clone.ServerPort).ShouldBe(42);

        // Cloned properties
        clone.IsLocal                            .ShouldBe(original.IsLocal);
        clone.IsFrozen                           .ShouldBe(original.IsFrozen); // diff behavior from Clone()
        clone.ServerName                         .ShouldBe(original.ServerName);
        clone.InstanceName                       .ShouldBe(original.InstanceName);
        clone.DatabaseName                       .ShouldBe(original.DatabaseName);
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
    public void Indexer_ScriptBlock_Null()
    {
        var original = MakeExampleContext();

        Should.Throw<ArgumentNullException>(
            () => original[(null as ScriptBlock)!]
        );
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Indexer_DatabaseName(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original["db2"];

        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(original);

        // Invariants
        clone.IsAzure                            .ShouldBeFalse();
        clone.AsAzure                            .ShouldBeNull();

        // Properties modified by script block
        clone.DatabaseName                       .ShouldBe("db2");

        // Cloned properties
        clone.IsLocal                            .ShouldBe(original.IsLocal);
        clone.IsFrozen                           .ShouldBe(original.IsFrozen); // diff behavior from Clone()
        clone.ServerName                         .ShouldBe(original.ServerName);
        clone.ServerPort                         .ShouldBe(original.ServerPort);
        clone.InstanceName                       .ShouldBe(original.InstanceName);
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
    public void Indexer_ServerName_DatabaseName(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original["srv2", "db2"];

        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(original);

        // Invariants
        clone.IsAzure                            .ShouldBeFalse();
        clone.AsAzure                            .ShouldBeNull();

        // Properties modified by script block
        clone.ServerName                         .ShouldBe("srv2");
        clone.DatabaseName                       .ShouldBe("db2");

        // Cloned properties
        clone.IsLocal                            .ShouldBe(original.IsLocal);
        clone.IsFrozen                           .ShouldBe(original.IsFrozen); // diff behavior from Clone()
        clone.ServerPort                         .ShouldBe(original.ServerPort);
        clone.InstanceName                       .ShouldBe(original.InstanceName);
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
        PropertyCase(c => c.ServerName,                         "server"),
        PropertyCase(c => c.ServerPort,                         (ushort?) 1234),
        PropertyCase(c => c.InstanceName,                       "instance"),
        PropertyCase(c => c.DatabaseName,                       "database"),
        PropertyCase(c => c.Credential,                         MakeCredential()),
        PropertyCase(c => c.EncryptionMode,                     EncryptionMode.Full),
        PropertyCase(c => c.ConnectTimeout,                     60.Seconds()),
        PropertyCase(c => c.ClientName,                         "client"),
        PropertyCase(c => c.ApplicationName,                    "application"),
        PropertyCase(c => c.ApplicationIntent,                  ApplicationIntent.ReadOnly),
        PropertyCase(c => c.ExposeCredentialInConnectionString, true),
        PropertyCase(c => c.EnableConnectionPooling,            true),
        PropertyCase(c => c.EnableMultipleActiveResultSets,     true),
    ];

    public static Case PropertyCase<T>(Expression<Func<SqlContext, T>> property, T value)
    {
        var memberExpression = (MemberExpression) property.Body;
        var propertyInfo     = (PropertyInfo)     memberExpression.Member;

        return new Case(propertyInfo, value);
    }

    public static PSCredential MakeCredential()
    {
        return new PSCredential("username", "password".Secure());
    }

    [Test]
    [TestCaseSource(nameof(PropertyCases))]
    public void Property_Set_NotFrozen(PropertyInfo property, object? value)
    {
        var context = new SqlContext();

        property.SetValue(context, value);

        property.GetValue(context).ShouldBe(value);
    }

    [Test]
    [TestCaseSource(nameof(PropertyCases))]
    public void Property_Set_Frozen(PropertyInfo property, object? value)
    {
        var context = new SqlContext();

        context.Freeze();

        Should.Throw<TargetInvocationException>( // due to reflection
            () => property.SetValue(context, value)
        )
        .InnerException.ShouldBeOfType<InvalidOperationException>()
        .Message.ShouldStartWith("The context is frozen and cannot be modified.");
    }

    [Test]
    [TestCase(null, ".")]
    [TestCase("a",  "a")]
    public void GetEffectiveServerName(string? serverName, string expected)
    {
        var context = new SqlContext { ServerName = serverName };

        context.GetEffectiveServerName().ShouldBe(expected);
    }
}
