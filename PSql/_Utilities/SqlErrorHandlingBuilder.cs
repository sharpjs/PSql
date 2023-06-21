// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Text;
using System.Text.RegularExpressions;

namespace PSql;

using static RegexOptions;

public class SqlErrorHandlingBuilder
{
    private readonly StringBuilder            _builder;
    private readonly List<(string, int, int)> _chunks;

    public SqlErrorHandlingBuilder()
    {
        _builder = new(Prologue, capacity: 4096);
        _chunks  = new(          capacity:    2);
    }

    public void StartNewBatch()
    {
        if (_chunks.Count == 0)
            return;

        FinalizeBatch();
        _chunks.Clear();
    }

    public void Append(string sql)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));

        Append(sql, 0, sql.Length);
    }

    public void Append(string sql, Capture capture)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));
        if (capture is null)
            throw new ArgumentNullException(nameof(capture));

        Append(sql, capture.Index, capture.Length);
    }

    public void Append(string sql, int index, int length)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));
        if (index < 0 || index >= sql.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (length < 0 || length > sql.Length - index)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (length == 0)
            return;

        _chunks.Add((sql, index, length));
    }

    private bool ShouldWrap()
    {
        foreach (var (sql, start, length) in _chunks)
            if (NoWrapRegex.Match(sql, start, length).Success)
                return false;

        return true;
    }

    private void FinalizeBatch()
    {
        _builder.AppendLine()
            .Append("    SET @__sql__ = N'");

        AppendChunksEscaped();

        _builder.AppendLine("';");

        if (ShouldWrap())
            _builder.AppendLine("    EXEC sp_executesql @__sql__;");
        else
            AppendChunks();
    }

    private void AppendChunks()
    {
        foreach (var (sql, start, length) in _chunks)
            _builder.Append(sql, start, length);
    }

    private void AppendChunksEscaped()
    {
        foreach (var (sql, start, length) in _chunks)
            AppendChunkEscaped(sql, start, length);
    }

    private void AppendChunkEscaped(string sql, int start, int length)
    {
        while (length > 0)
        {
            var index = sql.IndexOf('\'', start, length);

            if (index < 0)
            {
                _builder.Append(sql, start, length);
                return;
            }
            else
            {
                var count = index - start;
                _builder.Append(sql, start, count++).Append("''");
                start  += count;
                length -= count;
            }
        }
    }

    public string Complete()
    {
        StartNewBatch();

        _builder.Append(Epilogue);

        return _builder.ToString();
    }

    private static readonly Regex NoWrapRegex = new Regex(
        @"^--#[ \t]*NOWRAP[ \t]*\r?$",
        Options
    );

    private const RegexOptions Options
        = Multiline
        | IgnoreCase
        | CultureInvariant
        | ExplicitCapture
        | Compiled;

    private const string Prologue = 
        """
        DECLARE @__sql__ nvarchar(max);
        BEGIN TRY

        """;

    private const string Epilogue =
        """

        END TRY
        BEGIN CATCH
            PRINT N''
            PRINT N'An error occurred while executing this batch:';

            -- Print SQL in chunks of no more than 4000 characters to work around the
            -- SQL Server limit of 4000 nvarchars per PRINT.  Break chunks at line
            -- boundaries, if possible, but ensure that the whole batch gets printed.

            DECLARE
                @crlf  nvarchar(2) = CHAR(13) + CHAR(10),   -- end of line
                @pos   bigint      =  1,                    -- a char pos
                @start bigint      =  1,                    -- first char pos of chunk
                @end   bigint      = -1,                    -- first char pos after chunk
                @len   bigint      = LEN(@__sql__);         -- last  char pos of SQL

            IF LEFT(@__sql__, 2) != @crlf
                PRINT N'';

            WHILE @start <= @len
            BEGIN
                WHILE @end <= @len
                BEGIN
                    SET @pos = CHARINDEX(@crlf, @__sql__, @end + 2);
                    IF @pos = 0
                        SET @pos = @len + 1;

                    IF @pos <= @start + 4000
                        SET @end = @pos; -- line fits in this chunk
                    ELSE IF @start = @end
                        SET @end = @start + 4000; -- line won't fit in any chunk
                    ELSE
                        BREAK; -- chunk full
                END;

                PRINT SUBSTRING(@__sql__, @start, @end - @start);

                SET @start = @end + 2;
                SET @end   = @start;
            END;

            IF RIGHT(@__sql__, 2) != @crlf
                PRINT N'';

            THROW;
        END CATCH;

        """;
}
