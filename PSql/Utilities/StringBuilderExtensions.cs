// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Text;

namespace PSql;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendQuoted(
        this StringBuilder builder,
        ReadOnlySpan<char> chars,
        char               quote)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        return builder
            .Append(quote)
            .AppendEscaped(chars, quote)
            .Append(quote);
    }

    public static StringBuilder AppendEscaped(
        this StringBuilder builder,
        ReadOnlySpan<char> chars,
        char               escape)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        for (;;)
        {
            var i = chars.IndexOf(escape);

            if (i < 0)
                return builder.Append(chars);

            builder
                .Append(chars[..i])
                .Append(escape, 2);

            chars = chars[++i..];
        }
    }
}
