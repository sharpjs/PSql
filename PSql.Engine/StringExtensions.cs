// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Extension methods for <see langword="string"/>.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    ///   Checks whether the string is either <see langword="null"/> or empty.
    /// </summary>
    /// <param name="s">
    ///   The string to check.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if <paramref name="s"/> is either
    ///     <see langword="null"/> or empty;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s)
        => string.IsNullOrEmpty(s);

    /// <summary>
    ///   Replaces the string with <see langword="null"/> if it is empty.
    /// </summary>
    /// <param name="s">
    ///   The string to transform.
    /// </param>
    /// <returns>
    ///   <see langword="null"/> if <paramref name="s"/> is empty;
    ///   <paramref name="s"/> otherwise.
    /// </returns>
    public static string? NullIfEmpty(this string? s)
        => s.NullIf(string.Empty);

    /// <summary>
    ///   Replaces the string with <see langword="null"/> if it is equal to the
    ///   specified string.
    /// </summary>
    /// <param name="s">
    ///   The string to transform.
    /// </param>
    /// <param name="nullish">
    ///   The string against which to compare <paramref name="s"/>.
    /// </param>
    /// <returns>
    ///   <see langword="null"/>
    ///     if <paramref name="s"/> is equal to <paramref name="nullish"/>;
    ///   <paramref name="s"/>
    ///     otherwise.
    /// </returns>
    /// <remarks>
    ///   This method performs a case-sensitive ordinal comparison.
    /// </remarks>
    public static string? NullIf(this string? s, string? nullish)
        => s == nullish ? null : s;
}
