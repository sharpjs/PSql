// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Tests.Integration;

[SetUpFixture]
public static class IntegrationTestsSetup
{
    private const string
        PasswordName = "MSSQL_SA_PASSWORD";

    internal const ushort
        DefaultServerPort   = 1433,
        AlternateServerPort = 3341;

    internal static string? DefaultServerPassword   { get; private set; }
    internal static string? AlternateServerPassword { get; private set; }

    private static SqlServerContainer? _container;

    [OneTimeSetUp]
    public static void SetUp()
    {
        // Three scenarios are supported:
        //
        // A: Environment variable MSSQL_SA_PASSWORD present.
        // => Assume that a suitable local SQL Server instance is running
        //    already on ports 1433 and 3341.  Use the given password to
        //    authenticate as SA.
        //
        // B: Process listening on port 1433 but not 3341.
        // => Assume that a suitable local SQL Server instance is running on
        //    port 1433 and supports integrated authentication.  Start an
        //    ephemeral SQL Server container on port 3341 using a generated SA
        //    password.
        //
        // C: No process listening on port 1433 or 3341.
        // => Start an ephemeral SQL Server container on ports 1433 and 3341
        //    using a generated SA password.

        var password = Environment
            .GetEnvironmentVariable(PasswordName)
            .NullIfEmpty();

        if (password is not null)
        {
            // Scenario A
            DefaultServerPassword   = password;
            AlternateServerPassword = password;
        }
        else if (TcpPort.IsListening(DefaultServerPort))
        {
            // Scenario B
            _container              = new SqlServerContainer(AlternateServerPort);
            AlternateServerPassword = _container.Credential.Password;
        }
        else
        {
            // Scenario C
            _container              = new SqlServerContainer(DefaultServerPort, AlternateServerPort);
            DefaultServerPassword   = _container.Credential.Password;
            AlternateServerPassword = _container.Credential.Password;
        }
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        _container?.Dispose();
        _container = null;
    }
}
