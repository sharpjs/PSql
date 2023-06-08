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
///   This type implements the technique recommended
///   <a href="https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts">here</a>
///   to isolate dependencies of PSql so that they do not conflict with
///   dependencies of other modules.
/// </remarks>
public class PSqlDependencyResolutionHandler
    : IModuleAssemblyInitializer
    , IModuleAssemblyCleanup
{
    private static readonly object _importCountLock = new();
    private static          int    _importCount     = 0;

    // This is a technique to load PSql.private.dll and its dependencies into a
    // private AssemblyLoadContext to prevent a conflict if some other module
    // loads a different version of the same assembly.
    // See: https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts

    /// <summary>
    ///   Invoked by PowerShell when the module is imported into a runspace.
    /// </summary>
    void IModuleAssemblyInitializer.OnImport()
    {
        Initialize();
    }

    /// <summary>
    ///   Invoked by PowerShell when the module is removed from a runspace.
    /// </summary>
    /// <param name="module">
    ///   The module being removed.
    /// </param>
    void IModuleAssemblyCleanup.OnRemove(PSModuleInfo module)
    {
        CleanUp();
    }

    internal static void Initialize()
    {
        lock (_importCountLock)
        {
            if (_importCount++ > 0)
                return;

            AssemblyLoadContext.Default.Resolving += HandleResolving;
        }
    }

    internal static void CleanUp()
    {
        lock (_importCountLock)
        {
            if (--_importCount > 0)
                return;

            _importCount = 0;
            AssemblyLoadContext.Default.Resolving -= HandleResolving;
        }
    }

    private static Assembly? HandleResolving(AssemblyLoadContext context, AssemblyName name)
    {
        switch (name.Name)
        {
            // Assemblies that load explicitly into the private context
            case "PSql.private":
            case "PSql.Deploy.private":
                return PSqlAssemblyLoadContext.Instance.LoadFromAssemblyName(name);

            // Assemblies that load implicitly into the private context
            case "Microsoft.Data.SqlClient":
            case "Prequel":
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new InvalidOperationException(
                    $"Attempted to load private dependency {name.Name} into the default context. This is a bug."
                );

            // Assemblies that are OK to load into the default context
            default:
                return null;
        }
    }
}
