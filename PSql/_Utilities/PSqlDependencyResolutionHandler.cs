// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;
using System.Runtime.Loader;

using Debugger = System.Diagnostics.Debugger;

namespace PSql;

/// <summary>
///   Module initialization and cleanup handler that configures dependency
///   resolution.
/// </summary>
/// <remarks>
///   This type is part of the dependency isolation technique described
///   <a href="https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts">here</a>.
/// </remarks>
public class PSqlDependencyResolutionHandler
    : IModuleAssemblyInitializer
    , IModuleAssemblyCleanup
{
    private static readonly object _registrationLock  = new();
    private static          int    _registrationCount = 0;

    /// <summary>
    ///   Invoked by PowerShell when the module is imported into a runspace.
    /// </summary>
    void IModuleAssemblyInitializer.OnImport()
    {
        Register();
    }

    /// <summary>
    ///   Invoked by PowerShell when the module is removed from a runspace.
    /// </summary>
    /// <param name="module">
    ///   The module being removed.
    /// </param>
    void IModuleAssemblyCleanup.OnRemove(PSModuleInfo module)
    {
        Unregister();
    }

    /// <summary>
    ///   Registers the PSql dependency resolution handler.
    /// </summary>
    /// <remarks>
    ///   This method must be invoked prior to using types defined by
    ///   <c>PSql.private.dll</c> or by its dependencies.
    /// </remarks>
    internal static void Register()
    {
        lock (_registrationLock)
        {
            if (_registrationCount++ > 0)
                return;

            AssemblyLoadContext.Default.Resolving += HandleResolving;
        }
    }

    /// <summary>
    ///   Unregisters the PSql dependency resolution handler.
    /// </summary>
    internal static void Unregister()
    {
        lock (_registrationLock)
        {
            if (--_registrationCount > 0)
                return;

            _registrationCount = 0;
            AssemblyLoadContext.Default.Resolving -= HandleResolving;
        }
    }

    private static Assembly? HandleResolving(AssemblyLoadContext context, AssemblyName name)
    {
        switch (name.Name)
        {
            // Assemblies that load directly into the PSql private context
            case "PSql.private":
            case "PSql.Deploy.private":
                return PSqlAssemblyLoadContext.Instance.LoadFromAssemblyName(name);

#if DEBUG
            // If PSql attempts to load the below assemblies into the default
            // context, it is a bug.  However, PSql must not prevent other
            // modules from doing so.  Thus, check for this bug only in debug
            // builds.  If this bug occurs in release builds, it might manifest
            // as an assembly-not-found error or as a stack overflow.

            // Assemblies that load transitively into the PSql private context
            case "Microsoft.Data.SqlClient":
            case "Prequel":
                if (Debugger.IsAttached)
                    Debugger.Break();
                goto default;
#endif

            // Everything else
            default:
                return null;
        }
    }
}
