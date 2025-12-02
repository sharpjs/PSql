// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;

namespace PSql.Commands;

/// <summary>
///   The <c>Invoke-Sql</c> command.
///   Runs SQL scripts on SQL Server, Azure SQL Database, or compatible
///   systems.
/// </summary>
[Cmdlet(VerbsLifecycle.Invoke, "Sql", DefaultParameterSetName = ContextName)]
[OutputType(typeof(PSObject[]))]
public class InvokeSqlCommand : ConnectedCmdlet
{
    private const int DefaultTimeoutSeconds = 30;

    /// <summary>
    ///   <b>-Sql:</b>
    ///   SQL scripts(s) to run.
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
    ///   <b>-NoPreprocessing:</b>
    ///   Disables SQLCMD preprocessing support.
    /// </summary>
    [Parameter]
    [Alias("NoSqlCmdMode")]
    public SwitchParameter NoPreprocessing { get; set; }

    /// <summary>
    ///   <b>-NoErrorHandling:</b>
    ///   Disables the error-handling wrapper.
    /// </summary>
    [Parameter]
    public SwitchParameter NoErrorHandling { get; set; }

    /// <summary>
    ///   <b>-UseSqlTypes:</b>
    ///   Reads column values using native SQL Server types instead of .NET types.
    /// </summary>
    [Parameter]
    public SwitchParameter UseSqlTypes { get; set; }

    /// <summary>
    ///   <b>-Timeout:</b>
    ///   Maximum duration to wait for each SQL batch to complete.
    /// </summary>
    [Parameter]
    public TimeSpan? Timeout { get; set; }

    private int TimeoutSeconds
        => Timeout.HasValue ? (int) Timeout.Value.TotalSeconds : DefaultTimeoutSeconds;

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
        Run(ShouldUseErrorHandling
            ? () => ExecuteAsync(E.SqlErrorHandling.Apply(scripts))
            : () => ExecuteAsync(scripts)
        );

        WaitForAsyncActions();
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

    private async Task ExecuteAsync(IEnumerable<string> batches)
    {
        foreach (var batch in batches)
            await ExecuteAsync(batch);
    }

    private async Task ExecuteAsync(string batch)
    {
        AssumeBeginProcessingInvoked();

        await using var enumerator = await Connection.InnerConnection.ExecuteAndProjectAsync(
            batch, new PSObjectBuilder(), TimeoutSeconds, UseSqlTypes, CancellationToken
        );

        while (await enumerator.MoveNextAsync())
            WriteObject(enumerator.Current);
    }
}
