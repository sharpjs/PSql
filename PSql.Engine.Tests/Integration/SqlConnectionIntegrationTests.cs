// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Data.SqlTypes;

namespace PSql.Integration;

using static SqlCompareOptions;

[TestFixture]
public class SqlConnectionIntegrationTests
{
    [Test]
    public void ExecuteAndProjectTo_ClrTypes()
    {
        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        using var result = connection.ExecuteAndProjectTo(
            """
            SELECT * FROM (VALUES (N'a', 1), (N'b', 2)) AS T (S, X);
            SELECT * FROM (VALUES (N'c', 3), (N'd', 4)) AS T (S, Y);
            """,
            TestObjectBuilder.Instance
        );

        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", "a"), ("X", 1)]);
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", "b"), ("X", 2)]);
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", "c"), ("Y", 3)]);
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", "d"), ("Y", 4)]);
        result.MoveNext().ShouldBeFalse();
    }

    [Test]
    [SetCulture("kl-GL")] // Greenlandic
    public void ExecuteAndProjectTo_SqlTypes()
    {
        // NOTE: It appears that the current .NET culture is what determines
        // the collation of a SqlString.  Even a collation specified explicitly
        // in SQL batch gets overridden by the current .NET culture.

        const int
            GreenlandicLcid = 1135;

        static SqlString Greenlandic(string s)
            => new(s, GreenlandicLcid, IgnoreCase | IgnoreKanaType | IgnoreWidth);

        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        using var result = connection.ExecuteAndProjectTo(
            """
            SELECT * FROM (VALUES (N'a', 1), (N'b', 2)) AS T (S, X);
            SELECT * FROM (VALUES
                (N'c' COLLATE Latin1_General_100_CI_AI_SC_UTF8, 3),
                (N'd' COLLATE Latin1_General_100_CI_AI_SC_UTF8, 4)
            ) AS T (S, Y);
            """,
            TestObjectBuilder.Instance,
            useSqlTypes: true
        );

        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", Greenlandic("a")), ("X", new SqlInt32(1))]);
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", Greenlandic("b")), ("X", new SqlInt32(2))]);
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", Greenlandic("c")), ("Y", new SqlInt32(3))]);
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("S", Greenlandic("d")), ("Y", new SqlInt32(4))]);
        result.MoveNext().ShouldBeFalse();
    }

    [Test]
    public void ExecuteAndProjectTo_Exception()
    {
        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        using var result = connection.ExecuteAndProjectTo(
            """
            SELECT * FROM (VALUES (1/1)) AS T (X);
            SELECT * FROM (VALUES (1/0)) AS T (X);
            """,
            TestObjectBuilder.Instance
        );

        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe([("X", 1)]);
        Should.Throw<DataException>(() => result.MoveNext());
    }

    private sealed class TestObject
    {
        public List<(string Name, object? Value)> Properties { get; } = new();
    }

    private sealed class TestObjectBuilder : IObjectBuilder<TestObject>
    {
        public static TestObjectBuilder Instance { get; } = new();

        public TestObject NewObject()
            => new();

        public void AddProperty(TestObject obj, string name, object? value)
            => obj.Properties.Add((name, value));
    }
}
