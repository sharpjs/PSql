// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;

namespace PSql;

[Cmdlet(VerbsLifecycle.Invoke, "Sql", DefaultParameterSetName = ContextName)]
[OutputType(typeof(PSObject[]))]
public class InvokeSqlCommand : ConnectedCmdlet
{
    // -Sql
    [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
    public string?[]? Sql { get; set; }

    // -Define
    [Parameter(Position = 1)]
    public Hashtable? Define { get; set; }

    // -NoPreprocessing
    [Parameter]
    [Alias("NoSqlCmdMode")]
    public SwitchParameter NoPreprocessing { get; set; }

    // -NoErrorHandling
    [Parameter]
    public SwitchParameter NoErrorHandling { get; set; }

    // -UseSqlTypes
    [Parameter]
    public SwitchParameter UseSqlTypes { get; set; }

    // -Timeout
    [Parameter]
    public TimeSpan? Timeout { get; set; }

    private readonly SqlCmdPreprocessor _preprocessor;
    private          SqlCommand?        _command;

    private bool ShouldUsePreprocessing
        => !NoPreprocessing;

    private bool ShouldUseErrorHandling
        => !NoErrorHandling;

    public InvokeSqlCommand()
    {
        _preprocessor = new();
    }

    protected override void BeginProcessing()
    {
        // Will open a connection if one is not already open
        base.BeginProcessing();

        // Clear any errors left by previous commands on this connection
        // NULLS: Connection ensured not null by base.BeginProcessing
        Connection!.ClearErrors();

        _command = Connection.CreateCommand();

        if (Timeout.HasValue)
            _command.CommandTimeout = (int) Timeout.Value.TotalSeconds;
    }

    protected override void ProcessRecord()
    {
        // Check if scripts were provided at all
        if (Sql is not IEnumerable<string?> items)
            return;

        // No need to send empty scripts to server
        var scripts = ExcludeNullOrEmpty(items);

        // Add optional preprocessing
        if (ShouldUsePreprocessing)
            scripts = Preprocess(scripts);

        // Execute with optional error handling
        if (ShouldUseErrorHandling)
            Execute(SqlErrorHandling.Apply(scripts));
        else
            Execute(scripts);
    }

    private static IEnumerable<string> ExcludeNullOrEmpty(IEnumerable<string?> scripts)
    {
        return scripts.Where(s => !string.IsNullOrEmpty(s))!;
    }

    private IEnumerable<string> Preprocess(IEnumerable<string> scripts)
    {
        _preprocessor.SetVariables(Define);
        return scripts.SelectMany(s => _preprocessor.Process(s));
    }

    private void Execute(IEnumerable<string> batches)
    {
        foreach (var batch in batches)
            Execute(batch);
    }

    private void Execute(string batch)
    {
        // NULLS: _command created in BeginProcessing
        _command!.CommandText = batch;

        foreach (var obj in _command.ExecuteAndProjectToObjects(ObjectCreator, PropertySetter, UseSqlTypes))
            WriteObject(obj);
    }

    private static readonly Func<object>
        ObjectCreator = CreateObject;

    private static readonly Action<object, string, object?>
        PropertySetter = AddProperty;

    private static object CreateObject()
    {
        return new PSObject();
    }

    private static void AddProperty(object obj, string name, object? value)
    {
        var target   = (PSObject) obj;
        var property = new PSNoteProperty(name, value);

        target.Properties.Add(property);
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
        // NULLS: Connection ensured not null by BeginProcessing
        if (Connection!.HasErrors)
            throw new DataException("An error occurred while executing the SQL batch.");
    }

    protected override void Dispose(bool managed)
    {
        if (managed)
        {
            _command?.Dispose();
            _command = null;
        }

        base.Dispose(managed);
    }
}
