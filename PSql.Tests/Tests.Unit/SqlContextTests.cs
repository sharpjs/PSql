// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using FluentAssertions.Extensions;

namespace PSql.Tests.Unit;

using Case = TestCaseData;

[TestFixture]
public class SqlContextTests
{
    [Test]
    public void Defaults()
    {
        var context = new SqlContext();

        context.IsAzure                            .Should().BeFalse();
        context.AsAzure                            .Should().BeNull();
        context.IsLocal                            .Should().BeTrue();
        context.IsFrozen                           .Should().BeFalse();

        context.ServerName                         .Should().BeNull();
        context.ServerPort                         .Should().BeNull();
        context.InstanceName                       .Should().BeNull();
        context.DatabaseName                       .Should().BeNull();
        context.Credential                         .Should().BeNull();
        context.EncryptionMode                     .Should().Be(EncryptionMode.Default);
        context.ConnectTimeout                     .Should().BeNull();
        context.ClientName                         .Should().BeNull();
        context.ApplicationName                    .Should().BeNull();
        context.ApplicationIntent                  .Should().Be(ApplicationIntent.ReadWrite);
        context.ExposeCredentialInConnectionString .Should().BeFalse();
        context.EnableConnectionPooling            .Should().BeFalse();
        context.EnableMultipleActiveResultSets     .Should().BeFalse();
    }

    public void Freeze()
    {
        var context = new SqlContext();

        var frozen = context.Freeze();

        frozen         .Should().BeSameAs(context);
        frozen.IsFrozen.Should().BeTrue();
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

        clone.Should().NotBeNull();
        clone.Should().NotBeSameAs(original);
        clone.Should().BeEquivalentTo(original, o => o
            .Excluding(c => c.IsFrozen)
        );

        clone.IsFrozen.Should().BeFalse();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Indexer_ScriptBlock(bool frozen)
    {
        using var _  = new RunspaceScope();
        var original = MakeExampleContext(frozen);

        var clone = original[ScriptBlock.Create("$_.ServerPort = 42")];

        clone.Should().NotBeNull();
        clone.Should().NotBeSameAs(original);
        clone.Should().BeEquivalentTo(original, o => o
            .Excluding(c => c.ServerPort)
        );

        clone.ServerPort.Should().Be(42);
    }

    [Test]
    public void Indexer_ScriptBlock_Null()
    {
        var original = MakeExampleContext();

        original.Invoking(c => c[(null as ScriptBlock)!])
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Indexer_DatabaseName(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original["db2"];

        clone.Should().NotBeNull();
        clone.Should().NotBeSameAs(original);
        clone.Should().BeEquivalentTo(original, o => o
            .Excluding(c => c.DatabaseName)
        );

        clone.DatabaseName.Should().Be("db2");
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void Indexer_ServerName_DatabaseName(bool frozen)
    {
        var original = MakeExampleContext(frozen);

        var clone = original["srv2", "db2"];

        clone.Should().NotBeNull();
        clone.Should().NotBeSameAs(original);
        clone.Should().BeEquivalentTo(original, o => o
            .Excluding(c => c.ServerName)
            .Excluding(c => c.DatabaseName)
        );

        clone.ServerName  .Should().Be("srv2");
        clone.DatabaseName.Should().Be("db2");
    }

    public static readonly IEnumerable<Case> PropertyCases = new[]
    {
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
    };

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

        property.GetValue(context).Should().Be(value);
    }

    [Test]
    [TestCaseSource(nameof(PropertyCases))]
    public void Property_Set_Frozen(PropertyInfo property, object? value)
    {
        var context = new SqlContext();

        context.Freeze();

        context.Invoking(c => property.SetValue(context, value))
            .Should().Throw<TargetInvocationException>() // due to reflection
            .WithInnerException<InvalidOperationException>()
            .WithMessage("The context is frozen and cannot be modified.*");
    }

    [Test]
    [TestCase(null, ".")]
    [TestCase("a",  "a")]
    public void GetEffectiveServerName(string? serverName, string expected)
    {
        var context = new SqlContext { ServerName = serverName };

        context.GetEffectiveServerName().Should().Be(expected);
    }
}
