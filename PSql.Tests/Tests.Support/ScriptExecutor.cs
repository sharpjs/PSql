// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using Microsoft.PowerShell;
using Unindent;

namespace PSql.Tests;

using static FormattableString;

internal static class ScriptExecutor
{
    private static readonly InitialSessionState
        InitialState = CreateInitialSessionState();

    private static readonly string
        ScriptPreamble = Invariant($@"
            Set-Location ""{TestPath.EscapeForDoubleQuoteString()}""
        ").Unindent();

    private static string
        TestPath => TestContext.CurrentContext.TestDirectory;

    private static InitialSessionState CreateInitialSessionState()
    {
        var state = InitialSessionState.CreateDefault();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            state.ExecutionPolicy = ExecutionPolicy.RemoteSigned;

        state.Variables.Add(new SessionStateVariableEntry(
            "ErrorActionPreference", "Stop", description: null
        ));

        state.ImportPSModule(
            Path.Combine(TestPath, "PSql.psd1")
        );

        return state;
    }

    internal static (IReadOnlyList<PSObject?>, Exception?) Execute(string script)
    {
        if (script is null)
            throw new ArgumentNullException(nameof(script));

        script = ScriptPreamble + script.Unindent();

        var output    = new List<PSObject?>();
        var exception = null as Exception;

        using var shell = PowerShell.Create(InitialState);

        Redirect(shell.Streams, output);

        try
        {
            shell.AddScript(script).Invoke(input: null, output);
        }
        catch (Exception e)
        {
            exception = e;
        }

        return (output, exception);
    }

    internal static string EscapeForDoubleQuoteString(this string s)
        => s.Replace("\"", "`\"")
            .Replace("`",  "``");

    private static void Redirect(PSDataStreams streams, List<PSObject?> output)
    {
        streams.Warning.DataAdding += (_, data) => StoreWarning (data, output);
        streams.Error  .DataAdding += (_, data) => StoreError   (data, output);
    }

    private static void StoreWarning(DataAddingEventArgs data, List<PSObject?> output)
    {
        var written = (WarningRecord) data.ItemAdded;
        var message = new PSWarning(written.Message);
        output.Add(new PSObject(message));
    }

    private static void StoreError(DataAddingEventArgs data, List<PSObject?> output)
    {
        var written = (ErrorRecord) data.ItemAdded;
        var message = new PSError(written.Exception.Message);
        output.Add(new PSObject(message));
    }
}

internal record PSWarning (string Message);
internal record PSError   (string Message);
