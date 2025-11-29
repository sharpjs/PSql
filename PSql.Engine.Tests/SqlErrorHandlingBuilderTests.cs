// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;

namespace PSql.Tests.Unit;

[TestFixture]
public class SqlErrorHandlingBuilderTests
{
    [Test]
    public void Initial()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.IsEmpty   .ShouldBeTrue();
        builder.Complete().ShouldBeEmpty();
    }

    [Test]
    public void Append_Sql_Null()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentNullException>(() =>
        {
            builder.Append(null!);
        });
    }

    [Test]
    public void Append_Sql_Empty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("");
        builder.Append("");

        builder.IsEmpty   .ShouldBeTrue();
        builder.Complete().ShouldBeEmpty();
    }

    [Test]
    public void Append_Sql_NotEmpty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("fo");
        builder.Append("o");

        builder.IsEmpty   .ShouldBeFalse();
        builder.Complete().ShouldBe(ExpectedSingleBatch);
    }

    [Test]
    public void Append_Sql_NotEmpty_NoWrap()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("--# NOWRAP" + Environment.NewLine);
        builder.Append("fo");
        builder.Append("o");

        builder.IsEmpty   .ShouldBeFalse();
        builder.Complete().ShouldBe(ExpectedSingleBatchNoWrap);
    }

    [Test]
    public void Append_SqlCapture_NullSql()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentNullException>(() =>
        {
            builder.Append(null!, Regex.Match("foo", "foo"));
        });
    }

    [Test]
    public void Append_SqlCapture_NullCapture()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentNullException>(() =>
        {
            builder.Append("foo", null!);
        });
    }

    [Test]
    public void Append_SqlCapture_Empty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("foo", Regex.Match("foo", "not found"));
        builder.Append("foo", Regex.Match("foo", "not found"));

        builder.IsEmpty   .ShouldBeTrue();
        builder.Complete().ShouldBeEmpty();
    }

    [Test]
    public void Append_SqlCapture_NotEmpty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("foo", Regex.Match("foo", @"^fo"));
        builder.Append("foo", Regex.Match("foo", @"o$"));

        builder.IsEmpty   .ShouldBeFalse();
        builder.Complete().ShouldNotBeEmpty();
    }

    [Test]
    public void Append_SqlRange_NullSql()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentNullException>(() =>
        {
            builder.Append(null!, 0, 0);
        });
    }

    [Test]
    public void Append_SqlRange_IndexOutOfRangeLow()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentOutOfRangeException>(() =>
        {
            builder.Append("foo", -1, 0);
        });
    }

    [Test]
    public void Append_SqlRange_IndexOutOfRangeHigh()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentOutOfRangeException>(() =>
        {
            builder.Append("foo", 4, 0);
        });
    }

    [Test]
    public void Append_SqlRange_LengthOutOfRangeLow()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentOutOfRangeException>(() =>
        {
            builder.Append("foo", 0, -1);
        });
    }

    [Test]
    public void Append_SqlRange_LengthOutOfRangeHigh()
    {
        var builder = new SqlErrorHandlingBuilder();

        Should.Throw<ArgumentOutOfRangeException>(() =>
        {
            builder.Append("foo", 0, 4);
        });
    }

    [Test]
    public void Append_SqlRange_Empty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("foo", 0, 0);
        builder.Append("foo", 3, 0); // To test empty range at end

        builder.IsEmpty   .ShouldBeTrue();
        builder.Complete().ShouldBeEmpty();
    }

    [Test]
    public void Append_SqlRange_NotEmpty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("foo", 0, 2);
        builder.Append("foo", 2, 1);

        builder.IsEmpty   .ShouldBeFalse();
        builder.Complete().ShouldNotBeEmpty();
    }

    [Test]
    public void StartNewBatch_Empty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.StartNewBatch();

        builder.IsEmpty   .ShouldBeTrue();
        builder.Complete().ShouldBeEmpty();
    }

    [Test]
    public void StartNewBatch_NotEmpty()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("foo");
        builder.StartNewBatch();

        builder.IsEmpty   .ShouldBeFalse();
        builder.Complete().ShouldBe(ExpectedSingleBatch);
    }

    [Test]
    public void StartNewBatch_Multiple()
    {
        var builder = new SqlErrorHandlingBuilder();

        builder.Append("--# NOWRAP" + Environment.NewLine);
        builder.Append("'foo'"); // also tests that quotes are NOT escaped
        builder.StartNewBatch();
        builder.Append("'bar'"); // also tests that quotes ARE escaped

        builder.IsEmpty   .ShouldBeFalse();
        builder.Complete().ShouldBe(ExpectedMultipleBatch);
    }

    private const string ExpectedSingleBatch =
        """
        DECLARE @__sql__ nvarchar(max);
        BEGIN TRY

            SET @__sql__ = N'foo';
            EXEC sp_executesql @__sql__;

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

    private const string ExpectedSingleBatchNoWrap =
        """
        DECLARE @__sql__ nvarchar(max);
        BEGIN TRY

            SET @__sql__ = N'--# NOWRAP
        foo';
        --# NOWRAP
        foo
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

    private const string ExpectedMultipleBatch =
        """
        DECLARE @__sql__ nvarchar(max);
        BEGIN TRY

            SET @__sql__ = N'--# NOWRAP
        ''foo''';
        --# NOWRAP
        'foo'
            SET @__sql__ = N'''bar''';
            EXEC sp_executesql @__sql__;

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
