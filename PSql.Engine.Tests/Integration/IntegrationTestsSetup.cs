// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;

namespace PSql.Integration;

[SetUpFixture]
public static class IntegrationTestsSetup
{
    private const string
        PasswordName = "MSSQL_SA_PASSWORD",
        ServerPipe   = @"\\.\pipe\sql\query";

    private const ushort
        ServerPort = 1433;

    private static SqlServerContainer? _container;
    private static string?             _setupConnectionString;
    private static string?             _testConnectionString;
    private static NetworkCredential?  _credential;

    internal static string ConnectionString
        => _testConnectionString
        ?? throw new InvalidOperationException("ConnectionString not initialized.");

    internal static NetworkCredential? Credential
        => _credential;

    [OneTimeSetUp]
    public static void SetUp()
    {
        var connectionString = new SqlConnectionStringBuilder { DataSource = "." };

        var password = Environment
            .GetEnvironmentVariable(PasswordName)
            .NullIfEmpty();

        if (password is not null)
        {
            // Scenario A: Environment variable MSSQL_SA_PASSWORD present.
            // => Assume that a local SQL Server default instance is running.
            //    Use the given password to authenticate as SA.
            _credential = new NetworkCredential("sa", password);
        }
        else if (IsLocalSqlServerListening())
        {
            // Scenario B: Process listening on port 1433 or named pipe.
            // => Assume that a local SQL Server default instance is running
            //    and supports integrated authentication.  Assume that the
            //    current user has suffucient privileges to run tests.
            connectionString.IntegratedSecurity = true;
        }
        else
        {
            // Scenario C: No process listening on port 1433 or named pipe.
            // => Start an ephemeral SQL Server container on port 1433 using a
            //    generated SA password.
            _container  = new SqlServerContainer(ServerPort);
            _credential = _container.Credential;
        }

        connectionString.Encrypt         = SqlConnectionEncryptOption.Optional;
        connectionString.ApplicationName = "PSql.Deploy.Tests";

        _setupConnectionString = connectionString.ToString();

        connectionString.InitialCatalog = "PSqlDeployTest";

        _testConnectionString = connectionString.ToString();

        CreateTestDatabase();
    }


    private static bool IsLocalSqlServerListening()
    {
        return TcpPort.IsListening(ServerPort)
            || OperatingSystem.IsWindows() && File.Exists(ServerPipe);
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        try
        {
            RemoveTestDatabase();
        }
        finally
        {
            _container?.Dispose();
            _container = null;
        }
    }

    private static void CreateTestDatabase()
    {
        if (_setupConnectionString is null)
            return;

        using var connection = new SqlTestConnection(_setupConnectionString, _credential);

        connection.RemoveDatabase("PSqlDeployTest");
        connection.CreateDatabase("PSqlDeployTest");
    }

    private static void RemoveTestDatabase()
    {
        if (_setupConnectionString is null)
            return;

        using var connection = new SqlTestConnection(_setupConnectionString, _credential);

        connection.RemoveDatabase("PSqlDeployTest");
    }
}
