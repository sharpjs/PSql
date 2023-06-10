// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

[assembly: Parallelizable(ParallelScope.All)]

namespace PSql.Tests;

[SetUpFixture]
public static class TestSuite
{
    [OneTimeSetUp]
    public static void SetUp()
    {
        // Ensure that PSql.private.dll and its dependencies load correctly
        PSqlDependencyResolutionHandler.Register();
    }
}
