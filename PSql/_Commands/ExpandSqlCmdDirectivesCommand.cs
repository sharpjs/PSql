// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

using System.Collections;

namespace PSql;

[Cmdlet(VerbsData.Expand, "SqlCmdDirectives")]
[OutputType(typeof(string[]))]
public class ExpandSqlCmdDirectivesCommand : PSCmdlet
{
    // -Sql
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string?[]? Sql { get; set; }

    // -Define
    [Parameter(Position = 1)]
    public Hashtable? Define { get; set; }

    private readonly SqlCmdPreprocessor _preprocessor;

    public ExpandSqlCmdDirectivesCommand()
    {
        _preprocessor = new();
    }

    protected override void ProcessRecord()
    {
        if (Sql is not { } scripts)
            return;

        _preprocessor.SetVariables(Define);

        foreach (var script in scripts)
            if (script.HasContent())
                ProcessScript(script);
    }

    private void ProcessScript(string script)
    {
        foreach (var batch in _preprocessor.Process(script))
            if (batch.HasContent())
                WriteObject(batch);
    }
}
