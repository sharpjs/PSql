// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace PSql;

using static RuntimeInformation;

/// <summary>
///   A private <see cref="AssemblyLoadContext"/> that isolates dependencies
///   of PSql so that they do not conflict with dependencies of other modules.
/// </summary>
/// <remarks>
///   This type implements the technique recommended
///   <a href="https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts">here</a>.
/// </remarks>
internal sealed class PSqlAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    ///   Gets the singleton instance of <see cref="PSqlAssemblyLoadContext"/>.
    /// </summary>
    public static PSqlAssemblyLoadContext Instance { get; } = new PSqlAssemblyLoadContext();

    private readonly string _basePath;
    private readonly string _rid;

    private PSqlAssemblyLoadContext()
    {
        _basePath = Path.Combine(GetPSqlDirectory(), "deps");
        _rid      = GetRuntimeIdentifier();
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var assembly = LoadCore(assemblyName);
        if (assembly != null)
            Console.WriteLine("Loaded {0}", assembly.Location);
        return assembly;
    }

    private Assembly? LoadCore(AssemblyName assemblyName)
    {
        // This context resolves named assemblies only
        if (assemblyName.Name is not { } name)
            return null;

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
        var path = Path.Combine(_basePath, _rid, name);
        if (File.Exists(path))
            return LoadFromAssemblyPath(path);

        // Then, try general path
        path = Path.Combine(_basePath, name);
        if (File.Exists(path))
            return LoadFromAssemblyPath(path);

        // Else do not resolve
        return null;
    }

    private static string GetPSqlDirectory()
    {
        // NULLS: PSql.dll always is loaded from a DLL file (not a byte array).
        // Location is a valid file path, and GetDirectoryName returns non-null.
        return Path.GetDirectoryName(typeof(PSqlAssemblyLoadContext).Assembly.Location)!;
    }

    private static string GetRuntimeIdentifier()
    {
        return IsOSPlatform(OSPlatform.Windows) ? "win" : "unix";
    }
}
