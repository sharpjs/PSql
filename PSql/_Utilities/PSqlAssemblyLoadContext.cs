// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace PSql;

using Void = ValueTuple;

/// <summary>
///   A private <see cref="AssemblyLoadContext"/> that isolates dependencies
///   of PSql so that they do not conflict with dependencies of other modules.
/// </summary>
internal sealed class PSqlAssemblyLoadContext : AssemblyLoadContext
{
    // This isolation technique evolved from the one described here:
    // https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts

    /// <summary>
    ///   Gets the singleton instance of <see cref="PSqlAssemblyLoadContext"/>.
    /// </summary>
    public static PSqlAssemblyLoadContext Instance { get; } = new();

    private          AssemblyDependencyResolver[]         _resolvers;
    private readonly ConcurrentDictionary<string, Void>   _doneManaged;
    private readonly ConcurrentDictionary<string, IntPtr> _doneNative;

    private PSqlAssemblyLoadContext()
        : base(nameof(PSql))
    {
        _resolvers   = Array.Empty<AssemblyDependencyResolver>();
        _doneManaged = new(concurrencyLevel: 1, capacity: 40);
        _doneNative  = new(concurrencyLevel: 1, capacity:  4);
    }

    /// <summary>
    ///   Makes an assembly and the transitive dependencies described in its
    ///   accompanying <c>deps.json</c> file loadable in the current context.
    /// </summary>
    /// <param name="path">
    ///   The path of the assembly.  Both the assembly and its accompanying
    ///   <c>deps.json</c> file must exist.
    /// </param>
    public void AddResolutionSource(string path)
    {
        var oldArray = _resolvers;
        var resolver = new AssemblyDependencyResolver(path);

        for (;;)
        {
            // Replace array with one where path has been prepended
            var newArray = Prepend(resolver, oldArray);
            var original = Interlocked.CompareExchange(ref _resolvers, newArray, oldArray);
            if (original == oldArray)
                return; // was replaced

            // Other thread modified the array; retry
            oldArray = original;
        }
    }

    private static T[] Prepend<T>(T item, T[] array)
    {
        var result = new T[array.Length + 1];
        result[0] = item;
        Array.Copy(array, 0, result, 1, array.Length);
        return result;
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // This context loads named assemblies only
        if (assemblyName.Name is not { } name)
            return null;

        // Allow only one chance to load
        if (!_doneManaged.TryAdd(name, default))
            return null;

        // Try each resolver
        foreach (var resolver in _resolvers)
            if (resolver.ResolveAssemblyToPath(assemblyName) is { } path)
                return LoadFromAssemblyPath(path);

        // Fall back to other load contexts
        return null;
    }

    /// <inheritdoc/>
    protected override IntPtr LoadUnmanagedDll(string name)
    {
        // This method is invoked several times for the same DLL name, even if
        // that DLL was loaded previously.  Use caching to speed things up.

        // Return cached result on repeated invocations
        return _doneNative.GetOrAdd(name, LoadUnmanagedDllCore);
    }

    private IntPtr LoadUnmanagedDllCore(string name)
    {
        // Try each resolver
        foreach (var resolver in _resolvers)
            if (resolver.ResolveUnmanagedDllToPath(name) is { } path)
                return LoadUnmanagedDllFromPath(path);

        // Fall back to other load contexts
        return default;
    }
}
