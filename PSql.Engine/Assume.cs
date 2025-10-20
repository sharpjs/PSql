// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace PSql;

/// <summary>
///   Methods to indicate assumptions.
/// </summary>
[DebuggerNonUserCode]
[ExcludeFromCodeCoverage(Justification = "Uncoverable by design in Release build.")]
internal static class Assume
{
    /// <summary>
    ///   Assumes that the specified value is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    ///   In debug builds, this method validates the assumption at runtime,
    ///   throwing <see cref="InvalidOperationException"/> on violation.
    ///   In non-debug builds, this method has no runtime effect.
    /// </remarks>
    /// <param name="value">
    ///   The <paramref name="value"/> that is not <see langword="null"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///   <paramref name="value"/> was <see langword="null"/>.
    /// </exception>
    [Conditional("DEBUG")]
    public static void NotNull([NotNull] object? value)
    {
        if (value is null)
            throw new InvalidOperationException("A not-null assumption has been violated.");
    }

    /// <summary>
    ///   Assumes that the specified value is not <see langword="null"/>.
    /// </summary>
    /// <remarks>
    ///   In debug builds, this method validates the assumption at runtime,
    ///   throwing <see cref="InvalidOperationException"/> on violation.
    ///   In non-debug builds, this method has no runtime effect.
    /// </remarks>
    /// <typeparam name="T">
    ///   The underlying type of <paramref name="value"/>.
    /// </typeparam>
    /// <param name="value">
    ///   The <paramref name="value"/> that is not <see langword="null"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///   <paramref name="value"/> was <see langword="null"/>.
    /// </exception>
    [Conditional("DEBUG")]
    public static void NotNull<T>([NotNull] T? value)
        where T : struct
    {
        if (value is null)
            throw new InvalidOperationException("A not-null assumption has been violated.");
    }
}

