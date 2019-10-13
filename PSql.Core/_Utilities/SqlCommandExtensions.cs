using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;
using Microsoft.Data.SqlClient;
using static System.FormattableString;

namespace PSql
{
    internal static class SqlCommandExtensions
    {
        public static IEnumerable<PSObject> ExecuteAndProjectToPSObjects(
            this SqlCommand command,
            bool            useSqlTypes = false)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            using var reader = command.ExecuteReader();

            // Visit each result set
            do
            {
                var names = null as string[];

                // Visit each row in result set
                while (reader.Read())
                {
                    // If first row in result set, get column names
                    names ??= GetColumnNames(reader);

                    // Return the row as a PowerShell object
                    yield return useSqlTypes
                        ? ProjectWithSqlTypes(reader, names)
                        : ProjectWithClrTypes(reader, names);
                }
            }
            while (reader.NextResult());
        }

        private static string[] GetColumnNames(SqlDataReader reader)
        {
            var names = new string[reader.FieldCount];

            for (var i = 0; i < names.Length; i++)
                names[i] = reader.GetName(i).NullIfEmpty() ?? Invariant($"Col{i}");

            return names;
        }

        private static PSObject ProjectWithClrTypes(SqlDataReader reader, string[] names)
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

        private static PSObject ProjectWithSqlTypes(SqlDataReader reader, string[] names)
        {
            var obj = new PSObject();

            for (var i = 0; i < names.Length; i++)
            {
                obj.Properties.Add(new PSNoteProperty(
                    name:  names[i],
                    value: reader.GetSqlValue(i)
                ));
            }

            return obj;
        }
    }
}
