// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Text;
using System.Text.RegularExpressions;

namespace PSql;

using static RegexOptions;

/// <summary>
///   A builder that combines SQL batches into a single superbatch with an
///   error-handling wrapper that improves the diagnostic experience.
/// </summary>
public class SqlErrorHandlingBuilder
{
    private readonly StringBuilder            _builder;
    private readonly List<(string, int, int)> _chunks;

    /// <summary>
    ///   Initializes a new <see cref="SqlErrorHandlingBuilder"/> instance.
    /// </summary>
    public SqlErrorHandlingBuilder()
    {
        _builder = new(Prologue, capacity: 4096);
        _chunks  = new();
    }

    /// <summary>
    ///   Gets whether the builder is empty.
    /// </summary>
    public bool IsEmpty
        => SuperbatchIsEmpty
        && CurrentBatchIsEmpty;

    private bool SuperbatchIsEmpty
        => _builder.Length <= Prologue.Length;

    private bool CurrentBatchIsEmpty
        => _chunks.Count == 0;

    /// <summary>
    ///   Ends the current batch and begins a new batch.
    /// </summary>
    /// <remarks>
    ///   If the current batch is empty, this method has no effect.
    /// </remarks>
    public void StartNewBatch()
    {
        if (CurrentBatchIsEmpty)
            return;

        FinalizeBatch();
        _chunks.Clear();
    }

    /// <summary>
    ///   Appends the specified SQL code to the current batch.
    /// </summary>
    /// <param name="sql">
    ///   The SQL code to append to the current batch.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   If <paramref name="sql"/> is empty, this method has no effect.
    /// </remarks>
    public void Append(string sql)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));
        if (sql.Length == 0)
            return;

        Append(sql, 0, sql.Length);
    }

    /// <summary>
    ///   Appends the specified span of SQL code to the current batch.
    /// </summary>
    /// <param name="sql">
    ///   A string that contains SQL code to append to the current batch.
    /// </param>
    /// <param name="capture">
    ///   A regular expression capture whose <see cref="Capture.Index"/> and
    ///   <see cref="Capture.Length"/> properties specify the span of SQL code
    ///   within <paramref name="sql"/> to add to the current batch.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> and/or <paramref name="capture"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   If <paramref name="capture"/> zero-length, this method has no effect.
    /// </remarks>
    public void Append(string sql, Capture capture)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));
        if (capture is null)
            throw new ArgumentNullException(nameof(capture));

        Append(sql, capture.Index, capture.Length);
    }

    /// <summary>
    ///   Appends the specified span of SQL code to the current batch.
    /// </summary>
    /// <param name="sql">
    ///   A string that contains SQL code to append to the current batch.
    /// </param>
    /// <param name="index">
    ///   The index of the span of SQL code within <paramref name="sql"/> to
    ///   add to the current batch.
    /// </param>
    /// <param name="length">
    ///   The length of the span of SQL code within <paramref name="sql"/> to
    ///   add to the current batch.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="index"/> is negative or greater than the length of
    ///     <paramref name="sql"/>; or,
    ///   <paramref name="length"/> is negative or greater than the length of
    ///     the maximum span within <paramref name="sql"/> starting at
    ///     <paramref name="index"/>.
    /// </exception>
    /// <remarks>
    ///   If <paramref name="length"/> is zero, this method has no effect.
    /// </remarks>
    public void Append(string sql, int index, int length)
    {
        if (sql is null)
            throw new ArgumentNullException(nameof(sql));
        if (index < 0 || index > sql.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (length < 0 || length > sql.Length - index)
            throw new ArgumentOutOfRangeException(nameof(length));
        if (length == 0)
            return;

        _chunks.Add((sql, index, length));
    }

    /// <summary>
    ///   Ends the current batch, returns the accumulated superbatch with
    ///   error-handling wrapper, and resets the builder to its initial state.
    /// </summary>
    /// <returns>
    ///   The accumulated superbatch with error-handling wrapper.
    /// </returns>
    /// <remarks>
    ///   If the builder has accumulated no batches, then this method returns
    ///   an empty string.
    /// </remarks>
    public string Complete()
    {
        StartNewBatch();

        if (SuperbatchIsEmpty)
            return "";

        var result = _builder.Append(Epilogue).ToString();

        // Reset
        _builder.Clear();
        _builder.Append(Prologue);

        return result;
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

    private bool ShouldWrap()
    {
        foreach (var (sql, start, length) in _chunks)
            if (NoWrapRegex.Match(sql, start, length).Success)
                return false;

        return true;
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

    private static readonly Regex NoWrapRegex = new Regex(
        @"^--#[ \t]*NOWRAP[ \t]*\r?$",
        Multiline        |  // m: ^/$ match BOL/EOL
        IgnoreCase       |  // i: not case sensitive
        CultureInvariant |  //    invariant comparison
        Compiled            //    compile to an assembly
    );

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
