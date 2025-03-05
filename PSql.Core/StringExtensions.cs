// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace PSql;

/// <summary>
///   Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///   Checks whether the string is neither <see langword="null"/> nor empty
    ///   (<c>""</c>).
    /// </summary>
    /// <param name="s">
    ///   The string to check.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if <paramref name="s"/> is neither
    ///     <see langword="null"/> nor empty (<c>""</c>);
    ///   <see langword="false"/> otherwise.
    /// </returns>
    public static bool HasContent([NotNullWhen(true)] this string? s)
        => !string.IsNullOrEmpty(s);

    /// <summary>
    ///   Checks whether the string is either <see langword="null"/> or empty
    ///   (<c>""</c>).
    /// </summary>
    /// <param name="s">
    ///   The string to check.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if <paramref name="s"/> is either
    ///     <see langword="null"/> or empty (<c>""</c>);
    ///   <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s)
        => string.IsNullOrEmpty(s);

    /// <summary>
    ///   Replaces an empty string (<c>""</c>) with <see langword="null"/>.
    /// </summary>
    /// <param name="s">
    ///   The string to transform.
    /// </param>
    /// <returns>
    ///   <see langword="null"/> if <paramref name="s"/> is empty (<c>""</c>);
    ///   <paramref name="s"/> otherwise.
    /// </returns>
    public static string? NullIfEmpty(this string? s)
        => string.IsNullOrEmpty(s) ? null : s;
}
