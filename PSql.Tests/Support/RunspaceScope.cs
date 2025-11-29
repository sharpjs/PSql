// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Management.Automation.Runspaces;

namespace PSql;

internal sealed class RunspaceScope : IDisposable
{
    private readonly Runspace   _oldRunspace;
    private readonly PowerShell _shell;

    public RunspaceScope(InitialSessionState state)
        : this(PowerShell.Create(state)) { }

    public RunspaceScope()
        : this(PowerShell.Create()) { }

    private RunspaceScope(PowerShell shell)
    {
        _oldRunspace = Runspace.DefaultRunspace;
        _shell       = shell;

        // Set thread-static default runspace
        Runspace.DefaultRunspace = shell.Runspace;
    }

    void IDisposable.Dispose()
    {
        // Restore thread-static default runspace
        Runspace.DefaultRunspace = _oldRunspace;

        _shell.Dispose();
    }
}
