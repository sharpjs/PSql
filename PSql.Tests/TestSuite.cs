// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

extern alias Engine;

global using E = Engine::PSql;

using PSql.Internal;

[assembly: Parallelizable(ParallelScope.All)]

namespace PSql;

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
