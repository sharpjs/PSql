using System.Management.Automation;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace PSql
{
    [TestFixture]
    public class SqlContextTests
    {
        [Test]
        public void Defaults()
        {
            var context = new SqlContext();

            context.ServerName                         .Should().BeNull();
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
