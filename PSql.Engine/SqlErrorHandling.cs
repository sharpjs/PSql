// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Convenience methods for SQL error handling.
/// </summary>
public static class SqlErrorHandling
{
    /// <summary>
    ///   Combines the specified SQL batches into a single superbatch with an
    ///   error-handling wrapper that improves the diagnostic experience.
    /// </summary>
    /// <param name="batches">
    ///   The SQL batches to combine.
    /// </param>
    /// <returns>
    ///   A superbatch that consists of the given <paramref name="batches"/>
    ///   with an error-handling wrapper.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="batches"/> is <see langword="null"/>.
    /// </exception>
    public static string Apply(IEnumerable<string> batches)
    {
        if (batches is null)
            throw new ArgumentNullException(nameof(batches));

        var builder = new SqlErrorHandlingBuilder();

        foreach (var batch in batches)
        {
            builder.StartNewBatch();
            builder.Append(batch);
        }

        return builder.Complete();
    }
}
