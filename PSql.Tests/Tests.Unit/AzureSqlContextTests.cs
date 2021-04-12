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
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Extensions;
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

            context.ResourceGroupName                  .Should().BeNull();
            context.ServerName                         .Should().BeNull();
            context.ServerFullName                     .Should().BeNull();
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
                ResourceGroupName                  = "resource-group",
                ServerName                         = "server",
                ServerFullName                     = "server.example.com",
                ServerPort                         = 1234,
                InstanceName                       = "instance",
                DatabaseName                       = "database",
                AuthenticationMode                 = AzureAuthenticationMode.SqlPassword,
                Credential                         = credential,
                EncryptionMode                     = EncryptionMode.Unverified,
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
                .Excluding(c => c.ResourceGroupName)
                .Excluding(c => c.ServerName)
                .Excluding(c => c.DatabaseName)
            );

            clone.AsAzure          .Should().BeSameAs(clone);
            clone.ResourceGroupName.Should().Be("rg2");
            clone.ServerName       .Should().Be("srv2");
            clone.DatabaseName     .Should().Be("db2");
        }

        public static readonly IEnumerable<Case> PropertyCases = new[]
        {
            PropertyCase(c => c.ResourceGroupName,  "resource-group"),
            PropertyCase(c => c.AuthenticationMode, AzureAuthenticationMode.AadDeviceCodeFlow),
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
    }
}
