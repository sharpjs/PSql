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

using System.Management.Automation;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace PSql.Tests.Unit
{
    [TestFixture]
    public class SqlContextTests
    {
        [Test]
        public void Defaults()
        {
            var context = new SqlContext();

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

        [Test]
        public void Clone_Typed()
        {
            var credential = new PSCredential("username", "password".Secure());

            var original = new SqlContext
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

            var context = original.Clone();

            context.Should().NotBeNull().And.NotBeSameAs(original);

            context.ServerName                         .Should().Be("server");
            context.ServerPort                         .Should().Be(1234);
            context.InstanceName                       .Should().Be("instance");
            context.DatabaseName                       .Should().Be("database");
            context.Credential                         .Should().BeSameAs(credential);
            context.EncryptionMode                     .Should().Be(EncryptionMode.Full);
            context.ConnectTimeout                     .Should().Be(42.Seconds());
            context.ClientName                         .Should().Be("client");
            context.ApplicationName                    .Should().Be("application");
            context.ApplicationIntent                  .Should().Be(ApplicationIntent.ReadOnly);
            context.ExposeCredentialInConnectionString .Should().BeTrue();
            context.EnableConnectionPooling            .Should().BeTrue();
            context.EnableMultipleActiveResultSets     .Should().BeTrue();
        }
    }
}
