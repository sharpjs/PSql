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