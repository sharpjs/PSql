// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Data.SqlTypes;

namespace PSql.Integration;

using static SqlCompareOptions;

[TestFixture]
public class SqlConnectionIntegrationTests
{
    [Test]
    public void ExecuteAndProject_ClrTypes()
    {
        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        using var result = connection.ExecuteAndProject(
            """
            SELECT *, 10 FROM (VALUES (N'a', 1), (N'b', 2)) AS T (S, X);
            SELECT *, 20 FROM (VALUES (N'c', 3), (N'd', 4)) AS T (S, Y);
            """,
            TestObjectBuilder.Instance
        );

        ShouldHaveNext(result, ("S", "a"), ("X", 1), ("Col2", 10));
        ShouldHaveNext(result, ("S", "b"), ("X", 2), ("Col2", 10));
        ShouldHaveNext(result, ("S", "c"), ("Y", 3), ("Col2", 20));
        ShouldHaveNext(result, ("S", "d"), ("Y", 4), ("Col2", 20));
        ShouldNotHaveNext(result);
    }

    [Test]
    [SetCulture("kl-GL")] // Greenlandic
    public void ExecuteAndProject_SqlTypes()
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

        using var result = connection.ExecuteAndProject(
            """
            SELECT *, 10 FROM (VALUES (N'a', 1), (N'b', 2)) AS T (S, X);
            SELECT *, 20 FROM (VALUES
                (N'c' COLLATE Latin1_General_100_CI_AI_SC_UTF8, 3),
                (N'd' COLLATE Latin1_General_100_CI_AI_SC_UTF8, 4)
            ) AS T (S, Y);
            """,
            TestObjectBuilder.Instance,
            useSqlTypes: true
        );

        ShouldHaveNext(result, ("S", Greenlandic("a")), ("X", new SqlInt32(1)), ("Col2", new SqlInt32(10)));
        ShouldHaveNext(result, ("S", Greenlandic("b")), ("X", new SqlInt32(2)), ("Col2", new SqlInt32(10)));
        ShouldHaveNext(result, ("S", Greenlandic("c")), ("Y", new SqlInt32(3)), ("Col2", new SqlInt32(20)));
        ShouldHaveNext(result, ("S", Greenlandic("d")), ("Y", new SqlInt32(4)), ("Col2", new SqlInt32(20)));
        ShouldNotHaveNext(result);
    }

    [Test]
    public void ExecuteAndProject_Exception()
    {
        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        using var result = connection.ExecuteAndProject(
            """
            SELECT * FROM (VALUES (1/1)) AS T (X);
            SELECT * FROM (VALUES (1/0)) AS T (X);
            """,
            TestObjectBuilder.Instance
        );

        ShouldHaveNext(result, ("X", 1));
        Should.Throw<DataException>(() => result.MoveNext());
    }

    private static void ShouldHaveNext(
        IEnumerator<TestObject>               result,
        params (string Name, object? Value)[] properties)
    {
        result.MoveNext().ShouldBeTrue();
        result.Current.Properties.ShouldBe(properties);
        ((IEnumerator) result).Current.ShouldBeSameAs(result.Current);
    }

    private static void ShouldNotHaveNext(IEnumerator<TestObject> result)
    {
        result.MoveNext().ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => result.Current);
        Should.Throw<InvalidOperationException>(() => ((IEnumerator) result).Current);
        Should.Throw<NotSupportedException    >(() => result.Reset());
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
