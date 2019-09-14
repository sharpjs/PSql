using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Management.Automation;
using static System.FormattableString;

namespace PSql
{
    internal static class SqlCommandExtensions
    {
        public static IEnumerable<PSObject> ExecuteAndProjectToPSObjects(this SqlCommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            using (var reader = command.ExecuteReader())
            {
                // Visit each result set
                do
                {
                    var names = null as string[];

                    // Visit each row in result set
                    while (reader.Read())
                    {
                        // If first row in result set, get column names
                        if (names == null)
                            names = GetColumnNames(reader);

                        // Return the row as a PowerShell object
                        yield return Project(reader, names);
                    }
                }
                while (reader.NextResult());
            }
        }

        private static string[] GetColumnNames(SqlDataReader reader)
        {
            var names = new string[reader.FieldCount];

            for (var i = 0; i < names.Length; i++)
                names[i] = reader.GetName(i).NullIfEmpty() ?? Invariant($"Col{i}");

            return names;
        }

        private static PSObject Project(SqlDataReader reader, string[] names)
        {
            var obj = new PSObject();

            for (var i = 0; i < names.Length; i++)
            {
                obj.Properties.Add(new PSNoteProperty(
                    name:  names[i],
                    value: reader.IsDBNull(i) ? null : reader.GetValue(i)
                ));
            }

            return obj;
        }
    }
}
