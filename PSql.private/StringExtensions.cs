// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;

namespace PSql;

internal static class StringExtensions
{
    internal static bool HasContent([NotNullWhen(true)] this string? s)
        => !string.IsNullOrEmpty(s);

    internal static bool IsNullOrEmpty([NotNullWhen(false)] this string? s)
        => string.IsNullOrEmpty(s);

    internal static string? NullIfEmpty(this string? s)
        => string.IsNullOrEmpty(s) ? null : s;
}
