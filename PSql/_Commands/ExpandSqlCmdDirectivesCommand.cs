/*
    Copyright 2021 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Collections;
using System.Management.Automation;

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
