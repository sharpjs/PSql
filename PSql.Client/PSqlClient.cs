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
using System.Data;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Data.SqlClient;
using Path = System.IO.Path;

namespace PSql
{
    using static RuntimeInformation;

    /// <summary>
    ///   Top-level interface between PSql and PSql.Client.
    /// </summary>
    public class PSqlClient
    {
        private Action <string>                   WriteInformation { get; }
        private Action <string>                   WriteWarning     { get; }
        private Action <object>                   WriteOutput      { get; }
        private Func   <object>                   CreateObject     { get; }
        private Action <object, string, object?>  SetProperty      { get; }

        public PSqlClient(
            Action <string>                  writeInformation,
            Action <string>                  writeWarning,
            Action <object>                  writeOutput,
            Func   <object>                  createObject,
            Action <object, string, object?> setProperty)
        {
            WriteInformation = writeInformation
                ?? throw new ArgumentNullException(nameof(writeInformation));

            WriteWarning = writeWarning
                ?? throw new ArgumentNullException(nameof(writeWarning));

            WriteOutput = writeOutput
                ?? throw new ArgumentNullException(nameof(writeOutput));

            CreateObject = createObject
                ?? throw new ArgumentNullException(nameof(createObject));

            SetProperty = setProperty
                ?? throw new ArgumentNullException(nameof(setProperty));

            SniLoader.Load();

            // Test that we can create one
            using var connection = new SqlConnection("Server=.;Database=master;Integrated Security=True");
            connection.Open();

            using var command = connection.CreateCommand();
            command.Connection  = connection;
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT name FROM sys.types;";

            using var reader = command.ExecuteReader();

            while (reader.Read())
                reader.GetString(reader.GetOrdinal("name"));
        }

        public SqlConnectionStringBuilder CreateSqlConnectionStringBuilder()
            => new SqlConnectionStringBuilder();
    }
}
