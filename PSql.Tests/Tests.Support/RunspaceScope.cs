// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSql.Tests;

internal class RunspaceScope : IDisposable
{
    private readonly Runspace   _oldRunspace;
    private readonly PowerShell _shell;

    public RunspaceScope(InitialSessionState state)
        : this(PowerShell.Create(state)) { }

    public RunspaceScope()
        : this(PowerShell.Create()) { }

    protected RunspaceScope(PowerShell shell)
    {
        _oldRunspace = Runspace.DefaultRunspace;
        _shell       = shell;

        Runspace.DefaultRunspace = shell.Runspace;
    }

    void IDisposable.Dispose()
    {
        Runspace.DefaultRunspace = _oldRunspace;

        _shell.Dispose();
    }
}
