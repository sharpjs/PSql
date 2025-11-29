// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using PSql.Commands;

namespace PSql.Tests.Unit;

using static SqlClientVersion;
using static EncryptionMode;

using Case = TestCaseData;

[TestFixture]
public class SqlContextTests
{
    [Test]
    public void Construct_NullOther()
    {
        Should.Throw<ArgumentNullException>(
            () => new SqlContext(null!)
        );
    }

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
        context.EncryptionMode                     .ShouldBe(Default);
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
            EncryptionMode                     = Full,
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

        var clone = new SqlContext(original);

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

    private static void ShouldBeClone(object? obj, SqlContext original)
    {
        var clone = obj.ShouldBeOfType<SqlContext>();

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
    [TestCase(true )]
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
    [TestCase(true )]
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
    [TestCase(true )]
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
        PropertyCase(c => c.EncryptionMode,                     Full),
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
    [TestCase(null,            true )]
    [TestCase("",              true )]
    [TestCase(".",             true )]
    [TestCase("(local)",       true )]
    [TestCase("localhost",     true )]
    [TestCase("127.0.0.1",     true )]
    [TestCase("::1",           true )]
    [TestCase("s.example.com", false)]
    public void IsLocal_Get(string? serverName, bool isLocal)
    {
        var context = new SqlContext { ServerName = serverName };

        context.IsLocal.ShouldBe(isLocal);
    }

    [Test]
    public void IsLocal_Get_CurrentHostname()
    {
        var context = new SqlContext { ServerName = Dns.GetHostName() };

        context.IsLocal.ShouldBeTrue();
    }

    [Test]
    [TestCase(null, ".")]
    [TestCase("a",  "a")]
    public void GetEffectiveServerName(string? serverName, string expected)
    {
        var context = new SqlContext { ServerName = serverName };

        context.GetEffectiveServerName().ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_Default(SqlClientVersion version, string expected)
    {
        var context = new SqlContext();

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=s;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=s;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=s;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=s;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=s;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=s;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=s;Integrated Security=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=s;Integrated Security=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=s;Integrated Security=true;Pooling=false")]
    public void GetConnectionString_ExplicitServerName(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { ServerName = "s" };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, @"Data Source=.\I;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   @"Data Source=.\I;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, @"Data Source=.\I;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   @"Data Source=.\I;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, @"Data Source=.\I;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   @"Data Source=.\I;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   @"Data Source=.\I;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   @"Data Source=.\I;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, @"Data Source=.\I;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_ExplicitInstanceName(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { InstanceName = "I" };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.,3341;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.,3341;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.,3341;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.,3341;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.,3341;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.,3341;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.,3341;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.,3341;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.,3341;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_ExplicitServerPort(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { ServerPort = 3341 };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Initial Catalog=d;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Initial Catalog=d;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Initial Catalog=d;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Initial Catalog=d;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Initial Catalog=d;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Initial Catalog=d;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Initial Catalog=d;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Initial Catalog=d;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Initial Catalog=d;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_ExplicitDatabaseName(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { DatabaseName = "d" };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;User ID=u;Password=p;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;User ID=u;Password=p;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;User ID=u;Password=p;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;User ID=u;Password=p;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;User ID=u;Password=p;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;User ID=u;Password=p;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;User ID=u;Password=p;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;User ID=u;Password=p;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;User ID=u;Password=p;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_ExplicitCredential(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { Credential = new("u", "p".Secure()) };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_ExplicitCredential_Omit(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { Credential = new("u", "p".Secure()) };

        context.GetConnectionString(databaseName: null, version, omitCredential: true).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;User ID=u;Password=p;Persist Security Info=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;User ID=u;Password=p;Persist Security Info=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;User ID=u;Password=p;Persist Security Info=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;User ID=u;Password=p;Persist Security Info=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;User ID=u;Password=p;Persist Security Info=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;User ID=u;Password=p;Persist Security Info=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;User ID=u;Password=p;Persist Security Info=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;User ID=u;Password=p;Persist Security Info=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;User ID=u;Password=p;Persist Security Info=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    public void GetConnectionString_ExplicitCredential_Expose(SqlClientVersion version, string expected)
    {
        var context = new SqlContext
        {
            Credential                         = new("u", "p".Secure()),
            ExposeCredentialInConnectionString = true
        };

        // NOTE: Ignored because ExposeCredentialInConnectionString takes precedence
        //                                                       vvvvvvvvvvvvvvvvvvvv
        context.GetConnectionString(databaseName: null, version, omitCredential: true).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, None,       "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   None,       "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, None,       "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   None,       "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, None,       "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   None,       "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   None,       "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   None,       "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, None,       "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Legacy, Unverified, "Data Source=.;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1,   Unverified, "Data Source=.;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds1_1, Unverified, "Data Source=.;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Pooling=false")]
    [TestCase(Mds2,   Unverified, "Data Source=.;Integrated Security=true;Encrypt=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds2_1, Unverified, "Data Source=.;Integrated Security=true;Encrypt=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds3,   Unverified, "Data Source=.;Integrated Security=true;Encrypt=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds4,   Unverified, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5,   Unverified, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Mds5_2, Unverified, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Pooling=false")]
    [TestCase(Legacy, Full,       "Data Source=.;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds1,   Full,       "Data Source=.;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds1_1, Full,       "Data Source=.;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds2,   Full,       "Data Source=.;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds2_1, Full,       "Data Source=.;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds3,   Full,       "Data Source=.;Integrated Security=true;Encrypt=true;Pooling=false")]
    [TestCase(Mds4,   Full,       "Data Source=.;Integrated Security=true;Pooling=false")]
    [TestCase(Mds5,   Full,       "Data Source=.;Integrated Security=true;Pooling=false")]
    [TestCase(Mds5_2, Full,       "Data Source=.;Integrated Security=true;Pooling=false")]
    public void GetConnectionString_ExplicitEncryptionMode(SqlClientVersion version, EncryptionMode mode, string expected)
    {
        var context = new SqlContext { EncryptionMode = mode };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Connect Timeout=3;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Connect Timeout=3;Pooling=false")]
    public void GetConnectionString_ExplicitConnectTimeout(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { ConnectTimeout = 3.Seconds() };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Workstation ID=c;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Workstation ID=c;Pooling=false")]
    public void GetConnectionString_ExplicitClientName(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { ClientName = "c" };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Application Name=c;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Application Name=c;Pooling=false")]
    public void GetConnectionString_ExplicitApplicationName(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { ApplicationName = "c" };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;ApplicationIntent=ReadOnly;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true;ApplicationIntent=ReadOnly;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;ApplicationIntent=ReadOnly;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Application Intent=ReadOnly;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Application Intent=ReadOnly;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Application Intent=ReadOnly;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Application Intent=ReadOnly;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Application Intent=ReadOnly;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Application Intent=ReadOnly;Pooling=false")]
    public void GetConnectionString_ExplicitApplicationIntent(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { ApplicationIntent = ApplicationIntent.ReadOnly };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;Pooling=false")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;Pooling=false")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;Pooling=false")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Multiple Active Result Sets=true;Pooling=false")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Multiple Active Result Sets=true;Pooling=false")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true;Multiple Active Result Sets=true;Pooling=false")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Multiple Active Result Sets=true;Pooling=false")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Multiple Active Result Sets=true;Pooling=false")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true;Multiple Active Result Sets=true;Pooling=false")]
    public void GetConnectionString_ExplicitMultipleActiveResultSets(SqlClientVersion version, string expected)
    {
        var context = new SqlContext { EnableMultipleActiveResultSets = true };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    [TestCase(Legacy, "Data Source=.;Integrated Security=true;TrustServerCertificate=true")]
    [TestCase(Mds1,   "Data Source=.;Integrated Security=true;TrustServerCertificate=true")]
    [TestCase(Mds1_1, "Data Source=.;Integrated Security=true;TrustServerCertificate=true")]
    [TestCase(Mds2,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true")]
    [TestCase(Mds2_1, "Data Source=.;Integrated Security=true;Trust Server Certificate=true")]
    [TestCase(Mds3,   "Data Source=.;Integrated Security=true;Trust Server Certificate=true")]
    [TestCase(Mds4,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true")]
    [TestCase(Mds5,   "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true")]
    [TestCase(Mds5_2, "Data Source=.;Integrated Security=true;Encrypt=false;Trust Server Certificate=true")]
    public void GetConnectionString_ExplicitConnectionPooling(SqlClientVersion version, string expected)
    {
        var context = new SqlContext() { EnableConnectionPooling = true };

        context.GetConnectionString(databaseName: null, version).ShouldBe(expected);
    }

    [Test]
    public void Connect_NoCredential()
    {
        var context = new SqlContext
        {
            ServerName = "s",
        };

        using var connection = context.Connect(databaseName: null, Mock.Of<ICmdlet>());

        connection.InnerConnection.ConnectionString.ShouldBe(
            "Data Source=s;Integrated Security=true;Pooling=false"
        );
    }

    [Test]
    public void Connect_ExposedCredential()
    {
        var context = new SqlContext
        {
            ServerName                         = "s",
            Credential                         = new("u", "p".Secure()),
            ExposeCredentialInConnectionString = true,
        };

        using var connection = context.Connect(databaseName: null, Mock.Of<ICmdlet>());

        connection.InnerConnection.ConnectionString.ShouldBe(
            "Data Source=s;User ID=u;Password=p;Persist Security Info=true;Pooling=false"
        );
    }

    [Test]
    public void Connect_SeparateCredential()
    {
        var context = new SqlContext
        {
            ServerName = "s",
            Credential = new("u", "p".Secure()),
        };

        using var connection = context.Connect(databaseName: null, Mock.Of<ICmdlet>());

        connection.InnerConnection.ConnectionString.ShouldBe(
            "Data Source=s;Pooling=false"
        );
    }
}
