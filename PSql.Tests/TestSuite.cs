// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Internal;

[assembly: Parallelizable(ParallelScope.All)]

namespace PSql.Tests;

[SetUpFixture]
public static class TestSuite
{
    [OneTimeSetUp]
    public static void SetUp()
    {
        // Ensure that PSql.Engine.dll and its dependencies load correctly
        new ModuleLifecycleEvents().OnImport();
    }
}
