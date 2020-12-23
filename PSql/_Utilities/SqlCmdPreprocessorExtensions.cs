/*
    Copyright 2020 Jeffrey Sharp

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
using System.Collections;
using System.Globalization;

namespace PSql
{
    internal static class SqlCmdPreprocessorExtensions
    {
        public static SqlCmdPreprocessor WithVariables(
            this SqlCmdPreprocessor preprocessor, IDictionary? entries)
        {
            if (preprocessor is null)
                throw new ArgumentNullException(nameof(preprocessor));

            if (entries == null)
                return preprocessor;

            var variables = preprocessor.Variables;

            foreach (var obj in entries)
            {
                if (obj is not DictionaryEntry entry)
                    continue;

                if (entry.Key is null)
                    continue;

                var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);

                if (string.IsNullOrEmpty(key))
                    continue;

                var value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);

                variables[key] = value ?? string.Empty;
            }

            return preprocessor;
        }
    }
}
