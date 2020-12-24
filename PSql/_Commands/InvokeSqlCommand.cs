/*
    Copyright 2020 Jeffrey Sharp

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management.Automation;

namespace PSql
{
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

        private SqlCmdPreprocessor? _preprocessor;
        private SqlCommand?         _command;

        private bool ShouldUsePreprocessing
            => !NoPreprocessing;

        private bool ShouldUseErrorHandling
            => !NoErrorHandling;

        protected override void BeginProcessing()
        {
            // Will open a connection if one is not already open
            base.BeginProcessing();

            var connection = AssertWithinLifetime(Connection);

            // Clear any errors left by previous commands on this connection
            connection.ClearErrors();

            _preprocessor = new SqlCmdPreprocessor().WithVariables(Define);
            _command      = connection.CreateCommand(this);

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
            var preprocessor = AssertWithinLifetime(_preprocessor);

            return scripts.SelectMany(s => preprocessor.Process(s));
        }

        private void Execute(IEnumerable<string> batches)
        {
            foreach (var batch in batches)
                Execute(batch);
        }

        private void Execute(string batch)
        {
            var command = AssertWithinLifetime(_command);

            command.CommandText = batch;

            foreach (var obj in command.ExecuteAndProjectToPSObjects(UseSqlTypes))
                WriteObject(obj);
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
            var connection = AssertWithinLifetime(Connection);

            if (connection.HasErrors)
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
}
