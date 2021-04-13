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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Extensions;
using PS = System.Management.Automation;
using NUnit.Framework;

namespace PSql.Tests.Unit
{
    using Case = TestCaseData;

    [TestFixture]
    public class AzureSqlContextTests
    {
        [Test]
        public void Defaults()
        {
            var context = new AzureSqlContext();

            context.IsAzure                            .Should().BeTrue();
            context.AsAzure                            .Should().BeSameAs(context);
            context.IsLocal                            .Should().BeFalse();
            context.IsFrozen                           .Should().BeFalse();

            context.ServerResourceGroupName            .Should().BeNull();
            context.ServerResourceName                 .Should().BeNull();
            context.ServerName                         .Should().BeNull();
            context.ServerPort                         .Should().BeNull();
            context.InstanceName                       .Should().BeNull();
            context.DatabaseName                       .Should().BeNull();
            context.AuthenticationMode                 .Should().Be(AzureAuthenticationMode.Default);
            context.Credential                         .Should().BeNull();
            context.EncryptionMode                     .Should().Be(EncryptionMode.Full); // different from SqlContext
            context.ConnectTimeout                     .Should().BeNull();
            context.ClientName                         .Should().BeNull();
            context.ApplicationName                    .Should().BeNull();
            context.ApplicationIntent                  .Should().Be(ApplicationIntent.ReadWrite);
            context.ExposeCredentialInConnectionString .Should().BeFalse();
            context.EnableConnectionPooling            .Should().BeFalse();
            context.EnableMultipleActiveResultSets     .Should().BeFalse();
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

            clone.Should().NotBeNull();
            clone.Should().NotBeSameAs(original);
            clone.Should().BeEquivalentTo(original, o => o
                .Excluding(c => c.AsAzure)
                .Excluding(c => c.IsFrozen)
            );

            clone.AsAzure .Should().BeSameAs(clone);
            clone.IsFrozen.Should().BeFalse();
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Indexer_ResourceGroupName_ServerName_DatabaseName(bool frozen)
        {
            var original = MakeExampleContext(frozen);

            var clone = original["rg2", "srv2", "db2"];

            clone.Should().NotBeNull();
            clone.Should().NotBeSameAs(original);
            clone.Should().BeEquivalentTo(original, o => o
                .Excluding(c => c.AsAzure)
                .Excluding(c => c.ServerResourceGroupName)
                .Excluding(c => c.ServerResourceName)
                .Excluding(c => c.DatabaseName)
            );

            clone.AsAzure                .Should().BeSameAs(clone);
            clone.ServerResourceGroupName.Should().Be("rg2");
            clone.ServerResourceName     .Should().Be("srv2");
            clone.DatabaseName           .Should().Be("db2");
        }

        public static readonly IEnumerable<Case> PropertyCases = new[]
        {
            PropertyCase(c => c.ServerResourceGroupName, "resource-group"),
            PropertyCase(c => c.ServerResourceName,      "server"),
            PropertyCase(c => c.AuthenticationMode,      AzureAuthenticationMode.AadDeviceCodeFlow),
        };

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

            property.GetValue(context).Should().Be(value);
        }

        [Test]
        [TestCaseSource(nameof(PropertyCases))]
        public void Property_Set_Frozen(PropertyInfo property, object? value)
        {
            var context = new AzureSqlContext();

            context.Freeze();

            context.Invoking(c => property.SetValue(context, value))
                .Should().Throw<TargetInvocationException>() // due to reflection
                .WithInnerException<InvalidOperationException>()
                .WithMessage("The context is frozen and cannot be modified.*");
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

            context.GetEffectiveServerName().Should().Be(expected);
        }

        [Test]
        public void GetEffectiveServerName_NoServerResourceGroupName()
        {
            var context = new AzureSqlContext
            {
                ServerResourceGroupName = null,
                ServerResourceName      = GetAzSqlServerCommand.ExpectedServerName
            };

            context.Invoking(c => c.GetEffectiveServerName())
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void GetEffectiveServerName_NoServerResourceName()
        {
            var context = new AzureSqlContext
            {
                ServerResourceGroupName = GetAzSqlServerCommand.ExpectedResourceGroupName,
                ServerResourceName      = null
            };

            context.Invoking(c => c.GetEffectiveServerName())
                .Should().Throw<InvalidOperationException>();
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

            context.Invoking(c => c.GetEffectiveServerName())
                .Should().Throw<InvalidOperationException>();
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
                ResourceGroupName.Should().Be(ExpectedResourceGroupName);
                ServerName       .Should().Be(ExpectedServerName);

                WriteObject(SessionState.PSVariable.GetValue(ResultVariableName));
            }
        }
    }
}
