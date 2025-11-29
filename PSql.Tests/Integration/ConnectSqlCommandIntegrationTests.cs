// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Tests.Integration;

[TestFixture]
public class DisconnectSqlCommandIntegrationTests
{
    [Test]
    public void Invoke_Default()
    {
        Execute(
            """
            Disconnect-Sql
            """
        )
        .ShouldBeEmpty();
    }

    [Test]
    public void Invoke_Pipeline()
    {
        Execute(
            """
            $Connections = (Connect-Sql), $null, (Connect-Sql)
            $Connections | Disconnect-Sql
            $Connections
            """
        )
        .ShouldBe([false, null, false]);
    }

    [Test]
    public void Invoke_Connection()
    {
        Execute(
            """
            $Connections = $null, (Connect-Sql), (Connect-Sql)
            Disconnect-Sql -Connection $Connections
            $Connections
            """
        )
        .ShouldBe(
            [null, false, false]
        );
    }

    private static bool?[] Execute(string script)
    {
        var (output, exception) = ScriptExecutor.Execute(script);

        exception.ShouldBeNull();

        return output
            .Select(o => (SqlConnection?) o?.BaseObject)
            .Select(c => c?.InnerConnection.IsOpen)
            .ToArray();
    }
}
