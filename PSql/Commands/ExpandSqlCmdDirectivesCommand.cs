// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;

namespace PSql.Commands;

/// <summary>
///   The <c>Expand-SqlCmdDirectives</c> command.
///   Performs limited SQLCMD-style preprocessing.
/// </summary>
[Cmdlet(VerbsData.Expand, "SqlCmdDirectives")]
[OutputType(typeof(string[]))]
public class ExpandSqlCmdDirectivesCommand : PSqlCmdlet
{
    /// <summary>
    ///   <b>-Sql:</b>
    ///   SQL scripts(s) to process.
    /// </summary>
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string?[]? Sql { get; set; }

    /// <summary>
    ///   <b>-Define:</b>
    ///   SQLCMD preprocessor variables to define.
    /// </summary>
    [Parameter(Position = 1)]
    public Hashtable? Define { get; set; }

    /// <summary>
    ///   <b>-ReplaceVariablesInComments</b>
    ///   Perform SQLCMD variable replacement inside SQL comments.
    /// </summary>
    [Parameter]
    public SwitchParameter ReplaceVariablesInComments
    {
        get => _preprocessor.EnableVariableReplacementInComments;
        set => _preprocessor.EnableVariableReplacementInComments = value;
    }

    private readonly E.SqlCmdPreprocessor _preprocessor;

    /// <summary>
    ///   Initializes a new <see cref="ExpandSqlCmdDirectivesCommand"/>
    ///   instance.
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
