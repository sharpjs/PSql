using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.PowerShell;
using NUnit.Framework;
using static System.Char;

#nullable enable

namespace PSql
{
    internal static class ScriptExecutor
    {
        private static readonly InitialSessionState
            InitialState = CreateInitialSessionState();

        private static readonly string
            ScriptPreamble = $@"
                cd ""{TestPath.EscapeForDoubleQuoteString()}""
            ";

        private static string
            TestPath => TestContext.CurrentContext.TestDirectory;

        private static InitialSessionState CreateInitialSessionState()
        {
            var state = InitialSessionState.CreateDefault();

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

            script = ScriptPreamble + script;

            var output    = new List<PSObject?>();
            var exception = null as Exception;

            using var shell = PowerShell.Create(InitialState);

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

        internal static string Unindent(this string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // Take shortcut for empty strings
            if (s.Length == 0)
                return s;

            // Compute minimum indent of all lines
            var start  = 0;
            var indent = int.MaxValue;
            var count  = 0;
            do
            {
                var index = s.IndexOfNonWhitespace(start);
                indent    = Math.Min(indent, index - start);
                start     = s.IndexOfNextLine(index);
                count++;
            }
            while (start < s.Length);

            // Take shortcut for non-indented strings
            if (indent == 0)
                return s;

            // Build unindented string
            var result = new StringBuilder(s.Length - indent * count);
            start = indent;
            do
            {
                var index = s.IndexOfNextLine(start);
                result.Append(s, start, index - start);
                start = index + indent;
            }
            while (start < s.Length);

            return result.ToString();
        }

        private static int IndexOfNonWhitespace(this string s, int index)
        {
            while (index < s.Length && IsWhiteSpace(s, index)) { index++; }
            return index;
        }

        private static int IndexOfNextLine(this string s, int index)
        {
            while (index < s.Length && s[index++] != '\n') { }
            return index;
        }
    }
}
