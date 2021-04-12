using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSql.Tests
{
    internal class RunspaceScope : IDisposable
    {
        private readonly Runspace   _oldRunspace;
        private readonly PowerShell _shell;

        public RunspaceScope()
        {
            _oldRunspace = Runspace.DefaultRunspace;
            _shell       = PowerShell.Create();

            Runspace.DefaultRunspace = _shell.Runspace;
        }

        void IDisposable.Dispose()
        {
            Runspace.DefaultRunspace = _oldRunspace;

            _shell.Dispose();
        }
    }
}
