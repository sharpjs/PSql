// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;

namespace PSql.Commands;

/// <summary>
///   The <c>Expand-SqlCmdDirectives</c> command.
/// </summary>
[Cmdlet(VerbsData.Expand, "SqlCmdDirectives")]
[OutputType(typeof(string[]))]
public class ExpandSqlCmdDirectivesCommand : PSqlCmdlet
{
    /// <summary>
    ///   <b>-Sql:</b> TODO
    /// </summary>
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string?[]? Sql { get; set; }

    /// <summary>
    ///   <b>-Define:</b> TODO
    /// </summary>
    [Parameter(Position = 1)]
    public Hashtable? Define { get; set; }

    private readonly E.SqlCmdPreprocessor _preprocessor;

    /// <summary>
    ///   Creates a new <see cref="ExpandSqlCmdDirectivesCommand"/> instance.
    /// </summary>
    public ExpandSqlCmdDirectivesCommand()
    {
        _preprocessor = new();
    }

    /// <inheritdoc/>
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
