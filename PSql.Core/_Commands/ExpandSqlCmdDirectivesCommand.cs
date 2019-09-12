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
        [Alias("s")]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [AllowNull, AllowEmptyCollection]
        public string[] Sql { get; set; }

        // -Define
        [Alias("d")]
        [Parameter(Position = 1)]
        [AllowNull, AllowEmptyCollection]
        public Hashtable Define { get; set; }

        private SqlCmdPreprocessor _preprocessor;

        protected override void BeginProcessing()
        {
            _preprocessor = new SqlCmdPreprocessor();

            InitializeVariables(_preprocessor.Variables);
        }

        protected override void ProcessRecord()
        {
            var inputs = Sql;
            if (inputs == null)
                return;

            foreach (var input in inputs)
                if (!string.IsNullOrEmpty(input))
                    foreach (var batch in _preprocessor.Process(input))
                        WriteObject(batch);
        }

        private void InitializeVariables(IDictionary<string, string> variables)
        {
            var entries = Define;
            if (entries == null)
                return;

            foreach (DictionaryEntry entry in entries)
            {
                if (entry.Key is null)
                    continue;

                var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(key))
                    continue;

                var value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);

                variables[key] = value ?? string.Empty;
            }
        }
    }
}
