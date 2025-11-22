// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Runtime.Loader;
using PSql.Internal;

namespace PSql.Tests.Unit.Internal;

[TestFixture]
public class PrivateAssemblyLoadContextTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    public void HandleResolvingInDefaultLoadContext_NotPrivate()
    {
        PrivateAssemblyLoadContext
            .HandleResolvingInDefaultLoadContext(
                AssemblyLoadContext.Default,
                new("NotThePrivateAssembly")
            )
            .ShouldBeNull();
    }

    [Test]
    public void Load_Empty()
    {
        // Required to cover the case where AssemblyName.Name is null or empty.
        // As of .NET 6.0.36, AssemblyLoadContext will not exercise that case;
        // ALC will throw an exception if .Name is null or empty.  However, the
        // ALC documentation does not guarantee that behavior, so the class
        // under test should handle it as a matter of defensive programming.

        PrivateAssemblyLoadContext
            .Instance
            .SimulateLoad(new())
            .ShouldBeNull();
    }

    [Test]
    public void LoadUnmanagedDll_Found()
    {
        if (OperatingSystem.IsWindows())
        {
            PrivateAssemblyLoadContext
                .Instance
                .SimulateLoadUnmanagedDll("Microsoft.Data.SqlClient.SNI")
                .ShouldNotBe(IntPtr.Zero);

            // Exercise caching
            PrivateAssemblyLoadContext
                .Instance
                .SimulateLoadUnmanagedDll("Microsoft.Data.SqlClient.SNI")
                .ShouldNotBe(IntPtr.Zero);
        }
        else
        {
            Assert.Pass("This product ships no native libraries for non-Windows platforms.");
        }
    }

    [Test]
    public void LoadUnmanagedDll_NotFound()
    {
        PrivateAssemblyLoadContext
            .Instance
            .SimulateLoadUnmanagedDll("ThisWillNotBeFound")
            .ShouldBe(IntPtr.Zero);

        // Exercise caching
        PrivateAssemblyLoadContext
            .Instance
            .SimulateLoadUnmanagedDll("ThisWillNotBeFound")
            .ShouldBe(IntPtr.Zero);
    }

    [Test]
    public void RequireDirectoryPath_Empty()
    {
        Should.Throw<InvalidOperationException>(() =>
        {
            _ = PrivateAssemblyLoadContext.RequireDirectoryPath(null);
        });
    }
}
