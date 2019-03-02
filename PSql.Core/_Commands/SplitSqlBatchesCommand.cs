using System;
using System.Management.Automation;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.RegexOptions;

namespace PSql._Commands
{
    [Cmdlet(VerbsCommon.Split, "SqlBatches")]
    [OutputType(typeof(string[]))]
    public class SplitSqlBatchesCommand : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true)]
        [AllowNull, AllowEmptyCollection]
        public string[] Sql { get; set; }

        protected override void ProcessRecord()
        {
            if (Sql == null)
                return;

            foreach (var sql in Sql)
                if (!string.IsNullOrEmpty(sql))
                    ProcessRecord(sql);
        }

        private void ProcessRecord(string sql)
        {
            var start = 0;

            foreach (Match match in SqlBatchTokenRegex.Matches(sql))
            {
                if (!match.Value.StartsWith("G", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var batch = sql.Substring(start, match.Index - start);
                    start = match.Index + match.Length;

                WriteObject(batch);
            }

            if (start < sql.Length)
            {
                var batch = sql.Substring(start);

                WriteObject(batch);
            }
        }

        private static readonly Regex SqlBatchTokenRegex = new Regex(
            @"
                '   ( [^']  | ''   )*  ( '     | \z ) |     # string
                \[  ( [^\]] | \]\] )*  ( \]    | \z ) |     # quoted identifier
                --  .*?                ( \r?\n | \z ) |     # line comment
                /\* ( .     | \n   )*? ( \*/   | \z ) |     # block comment
                ^GO                    ( \r?\n | \z )       # batch separator
            ",
            Multiline               |
            IgnoreCase              |
            CultureInvariant        |
            IgnorePatternWhitespace |
            ExplicitCapture         |
            Compiled
        );
    }
}
