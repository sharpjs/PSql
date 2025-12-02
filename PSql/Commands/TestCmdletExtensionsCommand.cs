// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

[Cmdlet(VerbsDiagnostic.Test, "CmdletExtensions")]
public class TestCmdletExtensionsCommand : PSqlCmdlet
{
    public enum TestCase 
    {
        WriteHost,
    }

    [Parameter(Mandatory = true, Position = 0)]
    public TestCase Case { get; set; }

    [Parameter]
    public string? Message { get; set; }

    protected override void ProcessRecord()
    {
        switch (Case)
        {
            default: // case TestCase.WriteHost:
                WriteHost(Message); // implemented via extension method
                break;
        }
    }
}
