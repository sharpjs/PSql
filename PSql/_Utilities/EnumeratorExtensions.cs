// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

internal static class EnumeratorExtensions
{
    public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enumerator)
        => enumerator;
}
