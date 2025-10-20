// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Runtime.Loader;

namespace PSql.Internal;

using static PrivateAssemblyLoadContext;

/// <summary>
///   Handlers for module lifecycle events.
/// </summary>
public class ModuleLifecycleEvents : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
    /// <summary>
    ///   Invoked by PowerShell when the module is imported into a runspace.
    /// </summary>
    public void OnImport()
    {
        AssemblyLoadContext.Default.Resolving += HandleResolvingInDefaultLoadContext;
    }

    /// <summary>
    ///   Invoked by PowerShell when the module is removed from a runspace.
    /// </summary>
    /// <param name="module">
    ///   The module being removed.
    /// </param>
    public void OnRemove(PSModuleInfo module)
    {
        AssemblyLoadContext.Default.Resolving -= HandleResolvingInDefaultLoadContext;
    }
}
