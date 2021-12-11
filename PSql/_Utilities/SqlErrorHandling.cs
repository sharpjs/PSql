/*
    Copyright 2021 Jeffrey Sharp

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

using System.Text;
using System.Text.RegularExpressions;

namespace PSql;

using static RegexOptions;

public static class SqlErrorHandling
{
    public static string Apply(IEnumerable<string> batches)
    {
        var builder = new StringBuilder(4096);

        builder.Append(Prologue);

        foreach (var batch in batches)
        {
            builder
                .AppendLine()
                .Append("    SET @__sql__ = N'");

            var start = builder.Length;
            builder
                .Append(batch)
                .Replace("'", "''", start, builder.Length - start)
                .AppendLine("';");

            var exec = NoWrapRegex.IsMatch(batch)
                ? batch
                : "    EXEC sp_executesql @__sql__;";
            builder.AppendLine(exec);
        }

        builder.Append(Epilogue);

        return builder.ToString();
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
@"DECLARE @__sql__ nvarchar(max);
BEGIN TRY
";

    private const string Epilogue =
@"
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
";
}
