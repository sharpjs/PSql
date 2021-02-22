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

using System;
using System.Text;

namespace PSql
{
    using static Math;

    internal static class StringExtensions
    {
        internal static bool HasContent(this string? s)
            => !string.IsNullOrEmpty(s);

        internal static string? NullIfEmpty(this string? s)
            => string.IsNullOrEmpty(s) ? null : s;

        internal static string Unindent(this string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // Ignore leading newline, for ergonomics
            var skip = s.LengthOfLeadingNewLine();

            // Take shortcut for newline-only strings
            if (skip == s.Length)
                return string.Empty;

            // Compute minimum indent of all lines
            var start  = skip;
            var indent = int.MaxValue;
            var count  = 0;
            do
            {
                var index = s.IndexOfNonWhitespace(start);
                indent    = Min(indent, index - start);
                start     = s.IndexOfNextLine(index);
                count++;
            }
            while (start < s.Length);

            // Take shortcut for non-indented strings
            if (indent == 0)
                return s[skip..];

            // Build unindented string
            var result = new StringBuilder(s.Length - skip - indent * count);
            start = skip + indent;
            do
            {
                var index = s.IndexOfNextLine(start);
                result.Append(s, start, index - start);
                start = index + indent;
            }
            while (start < s.Length);

            return result.ToString();
        }

        private static int LengthOfLeadingNewLine(this string s)
        {
            return s.Length switch
            {
                >1 when (s[0], s[1]) is ('\r', '\n') => 2,
                >0 when (s[0]      ) is (      '\n') => 1,
                _                                    => 0
            };
        }

        private static int IndexOfNonWhitespace(this string s, int index)
        {
            while (index < s.Length && s.IsAsciiWhiteSpace(index)) { index++; }
            return index;
        }

        private static bool IsAsciiWhiteSpace(this string s, int index)
        {
            return s[index] switch
            {
                ' '  => true,
                '\t' => true,
                _    => false
            };
        }

        private static int IndexOfNextLine(this string s, int index)
        {
            while (index < s.Length && s[index++] != '\n') { }
            return index;
        }
    }
}
