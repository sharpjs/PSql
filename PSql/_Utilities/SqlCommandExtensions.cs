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
                    yield return ProjectToPSObject(reader, names, useSqlTypes);
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

        private static PSObject ProjectToPSObject(SqlDataReader reader, string[] names, bool useSqlTypes)
        {
            var obj = new PSObject();

            for (var i = 0; i < names.Length; i++)
            {
                obj.Properties.Add(new PSNoteProperty(
                    name:  names[i],
                    value: reader.GetValue(i, useSqlTypes)
                ));
            }

            return obj;
        }

        private static object GetValue(this SqlDataReader reader, int ordinal, bool useSqlTypes)
        {
            var value = useSqlTypes
                ? reader.GetSqlValue (ordinal)
                : reader.GetValue    (ordinal);

            return value is DBNull
                ? null
                : value;
        }
    }
}
