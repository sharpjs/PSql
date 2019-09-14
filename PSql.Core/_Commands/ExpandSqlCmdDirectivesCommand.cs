using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace PSql
{
    [Cmdlet(VerbsData.Expand, "SqlCmdDirectives")]
    [OutputType(typeof(string[]))]
    public class ExpandSqlCmdDirectivesCommand : Cmdlet
    {
        // -Sql
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public string[] Sql { get; set; }

        // -Define
        [Parameter(Position = 1)]
        public Hashtable Define { get; set; }

        private SqlCmdPreprocessor _preprocessor;

        protected override void BeginProcessing()
        {
            _preprocessor = new SqlCmdPreprocessor().WithVariables(Define);
        }

        protected override void ProcessRecord()
        {
            var scripts = Sql;
            if (scripts == null)
                return;

            foreach (var input in scripts)
                if (!string.IsNullOrEmpty(input))
                    foreach (var batch in _preprocessor.Process(input))
                        WriteObject(batch);
        }
    }
}
