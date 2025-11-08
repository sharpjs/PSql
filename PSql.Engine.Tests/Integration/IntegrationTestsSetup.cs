// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;
using Subatomix.Testing.SqlServerIntegration;

namespace PSql.Integration;

[SetUpFixture]
public static class IntegrationTestsSetup
{
    private static TemporaryDatabase? _database;

    public static TemporaryDatabase Database
        => _database ?? throw OnSetUpNotExecuted();

    public static NetworkCredential? Credential
        => TestSqlServer.Credential;

    [OneTimeSetUp]
    public static void SetUp()
    {
        TestSqlServer.SetUp();

        _database = TestSqlServer.CreateTemporaryDatabase();
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        TestSqlServer.TearDown();

        _database = null;
    }

    private static Exception OnSetUpNotExecuted()
    {
        return new InvalidOperationException(
            nameof(IntegrationTestsSetup) + "." + nameof(SetUp) + " has not executed."
        );
    }
}
