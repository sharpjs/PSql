// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;
using System.Runtime.Loader;

namespace PSql.Internal;

/// <summary>
///   Represents a private dependency assembly that loads, along with its
///   transitive dependencies, into a private <see cref="AssemblyLoadContext"/>
///   to prevent conflict with dependencies of other modules.
/// </summary>
/// <remarks>
///   All members of this type are thread-safe.
/// </remarks>
public class PrivateDependencyRegistration
{
    // This isolation technique evolved from teh one described here:
    // https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts

    private const string
        PrivateSubdirectoryName   = "deps",
        PrivateAssemblyNameSuffix = ".private",
        AssemblyExtension         = ".dll";

    private readonly string _name;
    private readonly object _lock;

    private uint _referenceCount;

    /// <summary>
    ///   Initializes a new <see cref="PrivateDependencyRegistration"/>
    ///   instance for the private dependency assembly corresponding to the
    ///   calling assembly.
    /// </summary>
    /// <remarks>
    ///   If the calling assembly is <c>X.dll</c>, then this constructor
    ///   requires an assembly <c>deps\X.private.dll</c> and accompanying
    ///   <c>deps\X.private.deps.json</c> file.
    /// </remarks>
    public PrivateDependencyRegistration()
    {
        var assembly = Assembly.GetCallingAssembly();

        _name = RequireName(assembly) + PrivateAssemblyNameSuffix;
        _lock = new();

        var path = Path.Combine(
            RequireDirectory(assembly),
            PrivateSubdirectoryName,
            _name + AssemblyExtension
        );

        PSqlAssemblyLoadContext.Instance.AddResolutionSource(path);
    }

    /// <summary>
    ///   Gets the simple name of the private dependency assembly.
    /// </summary>
    public string Name => _name;

    /// <summary>
    ///   Gets the current reference count.  The default is <c>0</c>
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When the reference count is above zero, the private dependency
    ///     assembly is loadable.  It and its transitive dependencies load into
    ///     the PSql private <see cref="AssemblyLoadContext"/>.
    ///   </para>
    ///   <para>
    ///     To change the reference count, use the <see cref="Reference"/> and
    ///     <see cref="Unreference"/> methods.
    ///   </para>
    /// </remarks>
    public uint ReferenceCount => _referenceCount;

    /// <summary>
    ///   Increments the reference count of the private dependency assembly.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When the reference count is above zero, the private dependency
    ///     assembly is loadable.  It and its transitive dependencies load into
    ///     the PSql private <see cref="AssemblyLoadContext"/>.
    ///   </para>
    ///   <para>
    ///     If <see cref="ReferenceCount"/> is <see cref="uint.MaxValue"/>,
    ///     this method has no effect.
    ///   </para>
    /// </remarks>
    public void Reference()
    {
        lock (_lock)
        {
            if (_referenceCount == uint.MaxValue)
                return;

            if (_referenceCount++ > 0)
                return;

            AssemblyLoadContext.Default.Resolving += HandleResolving;
        }
    }

    /// <summary>
    ///   Decrements the reference count of the private dependency assembly.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When the reference count is above zero, the private dependency
    ///     assembly is loadable.  It and its transitive dependencies load into
    ///     the PSql private <see cref="AssemblyLoadContext"/>.
    ///   </para>
    ///   <para>
    ///     If <see cref="ReferenceCount"/> is <see cref="uint.MinValue"/>,
    ///     this method has no effect.
    ///   </para>
    /// </remarks>
    public void Unreference()
    {
        lock (_lock)
        {
            if (_referenceCount == uint.MinValue)
                return;

            if (--_referenceCount > 0)
                return;

            AssemblyLoadContext.Default.Resolving -= HandleResolving;
        }
    }

    private Assembly? HandleResolving(AssemblyLoadContext context, AssemblyName request)
    {
        return request.Name == _name
            ? PSqlAssemblyLoadContext.Instance.LoadFromAssemblyName(request)
            : null;
    }

    private static string RequireName(Assembly assembly)
    {
        var name = assembly.GetName().Name;

        if (name.IsNullOrEmpty())
            throw new InvalidOperationException("The calling assembly must have a name.");

        return name;
    }

    private static string RequireDirectory(Assembly assembly)
    {
        var path = Path.GetDirectoryName(assembly.Location);

        if (path.IsNullOrEmpty())
            throw new InvalidOperationException("The calling assembly must have a directory path.");

        return path;
    }
}
