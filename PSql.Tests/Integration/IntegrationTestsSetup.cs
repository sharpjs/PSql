// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using Subatomix.Testing.SqlServerIntegration;

namespace PSql.Tests.Integration;

[SetUpFixture]
public static class IntegrationTestsSetup
{
    [OneTimeSetUp]
    public static void SetUp()
    {
        TestSqlServer.SetUp();
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        TestSqlServer.TearDown();
    }
}
