// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace PSql;

using static RuntimeInformation;
using Void = ValueTuple;

/// <summary>
///   A private <see cref="AssemblyLoadContext"/> that isolates dependencies
///   of PSql so that they do not conflict with dependencies of other modules.
/// </summary>
/// <remarks>
///   This type is part of the dependency isolation technique described
///   <a href="https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts">here</a>.
/// </remarks>
internal sealed class PSqlAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    ///   Gets the singleton instance of <see cref="PSqlAssemblyLoadContext"/>.
    /// </summary>
    public static PSqlAssemblyLoadContext Instance { get; } = new PSqlAssemblyLoadContext();

    private readonly string  _basePath;
    private readonly string  _os;
    private readonly string? _architecture;

    private readonly ConcurrentDictionary<string, Void>   _doneManaged;
    private readonly ConcurrentDictionary<string, IntPtr> _doneNative;

    private PSqlAssemblyLoadContext()
    {
        _basePath     = Path.Combine(GetPSqlDirectory(), "deps");
        _os           = GetOperatingSystem();
        _architecture = GetArchitecture();
        _doneManaged  = new(concurrencyLevel: 1, capacity: 40);
        _doneNative   = new(concurrencyLevel: 1, capacity:  4);
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // This context resolves named assemblies only
        if (assemblyName.Name is not { } name)
            return null;

        // Try to resolve an assembly only once
        if (!_doneManaged.TryAdd(name, default))
            return null;

        // Actually result
        return LoadCore(name);
    }

    private Assembly? LoadCore(string name)
    {
        // The default AssemblyLoadContext uses .NET Core/5+ default probing
        // behavior, featuring .deps.json files and a fairly complex directory
        // structure.  Custom contexts like this one inherit no such behavior.
        // To simplify implementation, this context expects a much simpler
        // directory structure:
        //
        // deps/        cross-platform managed assemblies
        //   unix/      managed assemblies for unix-likes
        //   win/       managed assemblies for Windows
        //     x86/     native assemblies for Windows x86 processes
        //     x64/     native assemblies for Windows x64 processes
        //     arm/     native assemblies for Windows 32-bit ARM processes
        //     arm64/   native assemblies for Windows 64-bit ARM processes

        // Assume assembly is in same-named DLL file
        name += ".dll";

        // First, try runtime-specific path
        var path = Path.Combine(_basePath, _os, name);
        if (File.Exists(path))
            return LoadFromAssemblyPath(path);

        // Then, try general path
        path = Path.Combine(_basePath, name);
        if (File.Exists(path))
            return LoadFromAssemblyPath(path);

        // Else do not resolve
        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string name)
    {
        // This method is invoked several times for the same DLL name, even if
        // that DLL was loaded previously.  Use caching to speed things up.

        // Return cached result on repeated invocations
        if (_doneNative.TryGetValue(name, out var result))
            return result;

        // Resolve only unmanaged DLLs known to ship with PSql
        result = name switch 
        {
            "Microsoft.Data.SqlClient.SNI.dll" => LoadUnmanagedDllCore(name),
            _                                  => default,
        };

        // Cache and return result
        return _doneNative.GetOrAdd(name, result);
    }

    private IntPtr LoadUnmanagedDllCore(string name)
    {
        // Is process architecture recognized?
        if (_architecture is null)
            return default; // no

        // Does PSql have a native DLL to load?
        var path = Path.Combine(_basePath, _os, _architecture, name);
        if (!File.Exists(path))
            return default; // no

        // Load
        return LoadUnmanagedDllFromPath(path);
    }

    private static string GetPSqlDirectory()
    {
        // NULLS: PSql.dll always is loaded from a DLL file (not a byte array).
        // Location is a valid file path, so GetDirectoryName returns non-null.
        return Path.GetDirectoryName(
            typeof(PSqlAssemblyLoadContext).Assembly.Location
        )!;
    }

    private static string GetOperatingSystem()
    {
        return IsOSPlatform(OSPlatform.Windows) ? "win" : "unix";
    }

    private static string? GetArchitecture()
    {
        return ProcessArchitecture switch
        {
            Architecture.X86   => "x86",
            Architecture.X64   => "x64",
            Architecture.Arm   => "arm",
            Architecture.Arm64 => "arm64",
            _                  => null
        };
    }
}
