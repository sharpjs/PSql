// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;
using Prequel;

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

    private SqlCmdPreprocessor? _preprocessor;

    protected override void BeginProcessing()
    {
        _preprocessor = new SqlCmdPreprocessor().WithVariables(Define);
    }

    protected override void ProcessRecord()
    {
        if (Sql is not string?[] scripts)
            return;

        foreach (var script in scripts)
            if (!string.IsNullOrEmpty(script))
                ProcessScript(script);
    }

    private void ProcessScript(string script)
    {
        // NULLS: _preprocessor created in BeginProcessing
        foreach (var batch in _preprocessor!.Process(script))
            WriteObject(batch);
    }
}
