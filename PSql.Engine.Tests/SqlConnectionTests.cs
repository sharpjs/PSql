// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

[TestFixture]
public class SqlConnectionTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    public async Task ExecuteAndProject_NegativeTimeoutAsync()
    {
        var builder = Mock.Of<IObjectBuilder<object>>();

        using var connection = new TestSqlConnection();

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() =>
            connection.ExecuteAndProjectAsync("any", builder, timeout: -1)
        );
    }

    [Test]
    public void UnexpectedDisposal()
    {
        using var connection = new TestSqlConnection();

        Should.Throw<DataException>(() =>
        {
            connection.SimulateUnexpectedDispose();
        });
    }

    private sealed class TestSqlConnection : SqlConnection
    {
        public TestSqlConnection()
            : base(
                "Server=.;Integrated Security=true",
                credential: null,
                NullSqlMessageLogger.Instance
            )
        { }

        public void SimulateUnexpectedDispose()
        {
            Connection.Dispose();
        }
    }
}
