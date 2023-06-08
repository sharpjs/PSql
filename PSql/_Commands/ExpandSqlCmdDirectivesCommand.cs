// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;

namespace PSql;

[Cmdlet(VerbsData.Expand, "SqlCmdDirectives")]
[OutputType(typeof(string[]))]
public class ExpandSqlCmdDirectivesCommand : Cmdlet
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
            if (!string.IsNullOrEmpty(script))
                ProcessScript(script);
    }

    private void ProcessScript(string script)
    {
        foreach (var batch in _preprocessor.Process(script))
            WriteObject(batch);
    }
}
