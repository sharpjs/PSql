// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Runtime.InteropServices;

namespace PSql;

// Microsoft.Data.SqlClient 2.0.0 and later, at least when used within this
// PowerShell module, has trouble locating the appropriate SNI DLL.  The
// workaround is to load it manually.

internal static class SniLoader
{
    internal static void Load()
    {
        // Does platform have a SNI DLL?
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return; // no

        // Get process architecture
        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86   => "x86",
            Architecture.X64   => "x64",
            Architecture.Arm   => "arm",
            Architecture.Arm64 => "arm64",
            _                  => null
        };

        // Does architcture have a SNI DLL?
        if (architecture == null)
            return; // no

        // Get path to SNI DLL
        var path = Path.Combine(
            "deps", "win", architecture, "Microsoft.Data.SqlClient.SNI.dll"
        );

        // Load SNI DLL
        // BUG: Still does not honor the AssemblyLoadContext in .NET Core 3.1
        // https://github.com/dotnet/runtime/issues/13819
        NativeLibrary.Load(
            path,
            typeof(SniLoader).Assembly,
            DllImportSearchPath.AssemblyDirectory
        );
    }
}
