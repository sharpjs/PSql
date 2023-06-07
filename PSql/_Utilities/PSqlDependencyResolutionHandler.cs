// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;
using System.Runtime.Loader;

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
    // This is a technique to load PSql.private.dll and its dependencies into a
    // private AssemblyLoadContext to prevent a conflict if some other module
    // loads a different version of the same assembly.
    // See: https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts

    /// <summary>
    ///   Invoked by PowerShell when the module is imported into a runspace.
    /// </summary>
    void IModuleAssemblyInitializer.OnImport()
    {
        AssemblyLoadContext.Default.Resolving += HandleResolving;
    }

    /// <summary>
    ///   Invoked by PowerShell when the module is removed from a runspace.
    /// </summary>
    /// <param name="module">
    ///   The module being removed.
    /// </param>
    void IModuleAssemblyCleanup.OnRemove(PSModuleInfo module)
    {
        AssemblyLoadContext.Default.Resolving -= HandleResolving;
    }

    private Assembly? HandleResolving(AssemblyLoadContext _, AssemblyName name)
    {
        switch (name.Name)
        {
            // Assemblies that must load into the private context
            case "Microsoft.Data.SqlClient":
            case "Prequel":
            case "PSql.private":
            case "PSql.Deploy.private":
                return PSqlAssemblyLoadContext.Instance.LoadFromAssemblyName(name);

            // Assemblies that are OK to load into the default context
            default:
                return null;
        }
    }
}
