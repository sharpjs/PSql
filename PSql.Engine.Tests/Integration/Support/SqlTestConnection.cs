// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;

namespace PSql.Integration;

internal class SqlTestConnection : SqlConnection
{
    public SqlTestConnection(string connectionString, NetworkCredential? credential)
        : base(connectionString, credential, TestSqlLogger.Instance) { }

    public void Execute(string sql)
    {
        SetUpCommand(sql, timeout: 30);
        AutoOpen();
        Command.ExecuteNonQuery();
        ThrowIfHasErrors();
    }

    public void CreateDatabase(string name)
    {
        var nameInQuote = name.Replace("]", "]]");

        Execute(
            $"""
            IF DB_ID(N'{name}') IS NULL EXEC(N'
                CREATE DATABASE [{nameInQuote}] COLLATE Latin1_General_100_CI_AI_SC_UTF8;
            ');
            """
        );
    }

    public void RemoveDatabase(string name)
    {
        var nameInString        = name        .Replace("'", "''");
        var nameInQuoteInString = nameInString.Replace("]", "]]");

        Execute(
            $"""
            IF DB_ID('{nameInString}') IS NOT NULL EXEC(N'
                ALTER DATABASE [{nameInQuoteInString}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP  DATABASE [{nameInQuoteInString}];
            ');
            """
        );
    }
}
