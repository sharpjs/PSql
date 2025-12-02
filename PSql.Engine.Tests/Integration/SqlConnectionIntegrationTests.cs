// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Data.SqlTypes;

namespace PSql.Integration;

using static SqlCompareOptions;

[TestFixture]
public class SqlConnectionIntegrationTests
{
    [Test]
    public async Task ExecuteAndProjectAsync_ClrTypes()
    {
        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        await using var result = await connection.ExecuteAndProjectAsync(
            """
            SELECT *, 10 FROM (VALUES (N'a', 1), (N'b', 2)) AS T (S, X);
            SELECT *, 20 FROM (VALUES (N'c', 3), (N'd', 4)) AS T (S, Y);
            """,
            TestObjectBuilder.Instance
        );

        await ShouldHaveNext(result, ("S", "a"), ("X", 1), ("Col2", 10));
        await ShouldHaveNext(result, ("S", "b"), ("X", 2), ("Col2", 10));
        await ShouldHaveNext(result, ("S", "c"), ("Y", 3), ("Col2", 20));
        await ShouldHaveNext(result, ("S", "d"), ("Y", 4), ("Col2", 20));
        await ShouldNotHaveNext(result);
    }

    [Test]
    [SetCulture("kl-GL")] // Greenlandic
    public async Task ExecuteAndProjectAsync_SqlTypesAsync()
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

        await using var result = await connection.ExecuteAndProjectAsync(
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

        await ShouldHaveNext(result, ("S", Greenlandic("a")), ("X", new SqlInt32(1)), ("Col2", new SqlInt32(10)));
        await ShouldHaveNext(result, ("S", Greenlandic("b")), ("X", new SqlInt32(2)), ("Col2", new SqlInt32(10)));
        await ShouldHaveNext(result, ("S", Greenlandic("c")), ("Y", new SqlInt32(3)), ("Col2", new SqlInt32(20)));
        await ShouldHaveNext(result, ("S", Greenlandic("d")), ("Y", new SqlInt32(4)), ("Col2", new SqlInt32(20)));
        await ShouldNotHaveNext(result);
    }

    [Test]
    public async Task ExecuteAndProjectAsync_ExceptionAsync()
    {
        using var connection = new SqlConnection(
            IntegrationTestsSetup.Database.ConnectionString,
            IntegrationTestsSetup.Credential,
            TestSqlLogger.Instance
        );

        await using var result = await connection.ExecuteAndProjectAsync(
            """
            SELECT * FROM (VALUES (1/1)) AS T (X);
            SELECT * FROM (VALUES (1/0)) AS T (X);
            """,
            TestObjectBuilder.Instance
        );

        await ShouldHaveNext(result, ("X", 1));
        await Should.ThrowAsync<DataException>(async () => await result.MoveNextAsync());
    }

    private static async ValueTask ShouldHaveNext(
        IAsyncEnumerator<TestObject>          result,
        params (string Name, object? Value)[] properties)
    {
        (await result.MoveNextAsync()).ShouldBeTrue();

        result.Current.Properties.ShouldBe(properties);
    }

    private static async ValueTask ShouldNotHaveNext(IAsyncEnumerator<TestObject> result)
    {
        (await result.MoveNextAsync()).ShouldBeFalse();

        Should.Throw<InvalidOperationException>(() => result.Current);
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
