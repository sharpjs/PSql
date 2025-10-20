// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace PSql.Internal;

using Void = ValueTuple;

/// <summary>
///   A private <see cref="AssemblyLoadContext"/> that isolates dependencies
///   of PSql so that they do not conflict with dependencies of other modules.
/// </summary>
internal sealed class PrivateAssemblyLoadContext : AssemblyLoadContext
{
    // Isolation technique evolved from the load-context-based example here:
    // https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts

    private const string
        PrivateSubdirectoryName = "deps",
        PrivateLoadContextName  = "PSql",
        PrivateAssemblyName     = "PSql.Engine", // was .private
        PrivateAssemblyFileName = PrivateAssemblyName + ".dll";

    /// <summary>
    ///   Gets the singleton instance of <see cref="PrivateAssemblyLoadContext"/>.
    /// </summary>
    internal static PrivateAssemblyLoadContext Instance { get; } = new();

    private readonly AssemblyDependencyResolver           _resolver;
    private readonly ConcurrentDictionary<string, Void>   _loadedManaged;
    private readonly ConcurrentDictionary<string, IntPtr> _loadedNative;

    /// <summary>
    ///   Initializes a new <see cref="PrivateAssemblyLoadContext"/> instance.
    /// </summary>
    private PrivateAssemblyLoadContext()
        : base(PrivateLoadContextName)
    {
        _resolver      = new(GetPrivateAssemblyPath());
        _loadedManaged = new(concurrencyLevel: 1, capacity: 40); // count seen in testing
        _loadedNative  = new(concurrencyLevel: 1, capacity:  4); // count seen in testing
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // This context loads named assemblies only
        if (assemblyName.Name is not { } name)
            return null;

        // Allow only one chance to load
        if (!_loadedManaged.TryAdd(name, default))
            return null;

        // Try to resolve and load
        if (_resolver.ResolveAssemblyToPath(assemblyName) is { } path)
            return LoadFromAssemblyPath(path);

        // Fall back to other load contexts
        return null;
    }

    /// <inheritdoc/>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        // This method is invoked several times for the same DLL name, even if
        // that DLL was loaded previously.  Use caching to speed things up.

        // Return cached result on repeated invocations
        return _loadedNative.GetOrAdd(unmanagedDllName, LoadUnmanagedDllCore);
    }

    [ExcludeFromCodeCoverage(
        Justification = "This product ships no unmanaged library for non-Windows platforms."
    )]
    private IntPtr LoadUnmanagedDllCore(string name)
    {
        // Try to resolve and load
        if (_resolver.ResolveUnmanagedDllToPath(name) is { } path)
            return LoadUnmanagedDllFromPath(path);

        // Fall back to other load contexts
        return default;
    }

    private static string GetPrivateAssemblyPath()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var path = RequireDirectoryPath(assembly.Location);

        return Path.Combine(path, PrivateSubdirectoryName, PrivateAssemblyFileName);
    }

    // Made internal for testing
    internal static string RequireDirectoryPath(string? path)
    {
        path = Path.GetDirectoryName(path);

        if (path.IsNullOrEmpty())
            throw new InvalidOperationException("The executing assembly must have a directory path.");

        return path;
    }

    /// <summary>
    ///   Handles the <see cref="AssemblyLoadContext.Resolving"/> event fired
    ///   from the <see cref="AssemblyLoadContext.Default"/> instance.
    /// </summary>
    /// <remarks>
    ///   This handler redirects requests for the private dependencies assembly
    ///   to <see cref="Instance"/>, so that the private context loads both the
    ///   private assembly and all its dependencies not already loaded in the
    ///   default context.
    /// </remarks>
    /// <param name="context">
    ///   The assembly load context that fired the event.
    ///   Should be <see cref="AssemblyLoadContext.Default"/>.
    /// </param>
    /// <param name="request">
    ///   The requested assembly assembly name.
    /// </param>
    /// <returns>
    ///   The result of loading the private dependencies assembly, if
    ///     <paramref name="request"/> identifies that assembly;
    ///   <see langword="null"/> otherwise.
    /// </returns>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter",
        Justification = "Must conform to AssemblyLoadContext.Resolving signature.")]
    internal static Assembly? HandleResolvingInDefaultLoadContext(
        AssemblyLoadContext context,
        AssemblyName        request)
    {
        return request.Name is PrivateAssemblyName
            ? Instance.LoadFromAssemblyName(request)
            : null;
    }

    // For testing
    internal Assembly? SimulateLoad(AssemblyName name)
    {
        return Load(name);
    }

    // For testing
    internal IntPtr SimulateLoadUnmanagedDll(string name)
    {
        return LoadUnmanagedDll(name);
    }
}
