// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

// Don't require doc comments.  Commands are documented via a help file.
#pragma warning disable CS1591

using System.Collections;

namespace PSql.Commands;

/// <summary>
///   The <c>Invoke-Sql</c> command.
/// </summary>
[Cmdlet(VerbsLifecycle.Invoke, "Sql", DefaultParameterSetName = ContextName)]
[OutputType(typeof(PSObject[]))]
public class InvokeSqlCommand : ConnectedCmdlet
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

    /// <summary>
    ///   <b>-NoPreprocessing:</b> TODO
    /// </summary>
    [Parameter]
    [Alias("NoSqlCmdMode")]
    public SwitchParameter NoPreprocessing { get; set; }

    /// <summary>
    ///   <b>-NoErrorHandling:</b> TODO
    /// </summary>
    [Parameter]
    public SwitchParameter NoErrorHandling { get; set; }

    /// <summary>
    ///   <b>-UseSqlTypes:</b> TODO
    /// </summary>
    [Parameter]
    public SwitchParameter UseSqlTypes { get; set; }

    /// <summary>
    ///   <b>-Timeout:</b> TODO
    /// </summary>
    [Parameter]
    public TimeSpan? Timeout { get; set; }

    private readonly E.SqlCmdPreprocessor _preprocessor;

    private bool ShouldUsePreprocessing
        => !NoPreprocessing;

    private bool ShouldUseErrorHandling
        => !NoErrorHandling;

    public InvokeSqlCommand()
    {
        _preprocessor = new();
    }

    protected override void ProcessRecord()
    {
        // Check if scripts were provided at all
        if (Sql is not { } items)
            return;

        // No need to send empty scripts to server
        var scripts = ExcludeNullOrEmpty(items);

        // Add optional preprocessing
        if (ShouldUsePreprocessing)
            scripts = ExcludeNullOrEmpty(Preprocess(scripts));

        // Execute with optional error handling
        if (ShouldUseErrorHandling)
            Execute(SqlErrorHandling.Apply(scripts));
        else
            Execute(scripts);
    }

    private static IEnumerable<string> ExcludeNullOrEmpty(IEnumerable<string?> scripts)
    {
        return scripts.Where(s => s.HasContent())!;
    }

    private IEnumerable<string> Preprocess(IEnumerable<string> scripts)
    {
        _preprocessor.SetVariables(Define);

        return scripts.SelectMany(_preprocessor.Process);
    }

    private void Execute(IEnumerable<string> batches)
    {
        foreach (var batch in batches)
            Execute(batch);
    }

    private void Execute(string batch)
    {
        foreach (var obj in ExecuteAndProjectToObjects(batch))
            WriteObject(obj);
    }

    private IEnumerator<object> ExecuteAndProjectToObjects(string batch)
    {
        const int DefaultTimeoutSeconds = 30;

        var timeout = Timeout.HasValue
            ? (int) Timeout.Value.TotalSeconds
            : DefaultTimeoutSeconds;

        // NULLS: _command created in BeginProcessing
        return Connection!.InnerConnection.ExecuteAndProjectTo(
            batch, new PSObjectBuilder(), timeout, UseSqlTypes
        );
    }

    protected override void EndProcessing()
    {
        base.EndProcessing();
        ReportErrors();
    }

    protected override void StopProcessing()
    {
        base.StopProcessing();
        ReportErrors();
    }

    private void ReportErrors()
    {
        // TODO: This should be done internally I think
        //// NULLS: Connection ensured not null by BeginProcessing
        //Connection!.InnerConnection.ThrowIfHasErrors();
    }
}
