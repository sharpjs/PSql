// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Integration;

[TestFixture]
public class ConnectSqlCommandIntegrationTests
{
    [Test]
    public void Invoke_Default()
    {
        Execute(
            "Connect-Sql"
        )
        .ShouldBe(
            "Data Source=.;Integrated Security=true;" +
            "Encrypt=false;Trust Server Certificate=true;Pooling=false"
        );
    }

    [Test]
    public void Invoke_Pipeline()
    {
        Execute(
            "New-SqlContext -ReadOnlyIntent | Connect-Sql"
        )
        .ShouldBe(
            "Data Source=.;Integrated Security=true;" +
            "Encrypt=false;Trust Server Certificate=true;" +
            "Application Intent=ReadOnly;Pooling=false"
        );
    }

    [Test]
    public void Invoke_Context()
    {
        Execute(
            "Connect-Sql -Context (New-SqlContext -ReadOnlyIntent)"
        )
        .ShouldBe(
            "Data Source=.;Integrated Security=true;" +
            "Encrypt=false;Trust Server Certificate=true;" +
            "Application Intent=ReadOnly;Pooling=false"
        );
    }

    [Test]
    public void Invoke_DatabaseName()
    {
        Execute(
            "New-SqlContext -ReadOnlyIntent | Connect-Sql -DatabaseName master"
        )
        .ShouldBe(
            "Data Source=.;Initial Catalog=master;Integrated Security=true;" +
            "Encrypt=false;Trust Server Certificate=true;" +
            "Application Intent=ReadOnly;Pooling=false"
        );
    }

    private static string Execute(string script)
    {
        var (output, exception) = ScriptExecutor.Execute(script);

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldBeOfType<PSObject>()
            .BaseObject.ShouldBeOfType<SqlConnection>().AssignTo(out var connection);

        try
        {
            return connection.InnerConnection.ConnectionString;
        }
        finally
        {
            connection.Dispose();
        }
    }
}
