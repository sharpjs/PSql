using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.RegexOptions;

namespace PSql
{
    internal class SqlCmdPreprocessor
    {
        private readonly Dictionary<string, string> _variables;
        private          StringBuilder              _builder;

        public SqlCmdPreprocessor()
        {
            _variables = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Variables
            => _variables;

        public IEnumerable<string> Process(string text, string name = null)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (text.Length == 0)
                return Enumerable.Empty<string>();

            return ProcessCore(new Input(text, name ?? "(script)"));
        }

        private IEnumerable<string> ProcessCore(Input input)
        {
            var start = 0;
            string batch;

            do
            {
                (batch, start) = GetNextBatch(input, start);
                yield return batch;
            }
            while (start < input.Text.Length);
        }

        private (string sql, int next) GetNextBatch(Input input, int start)
        {
            // Verbatim mode - reuse input string if possible

            var text  = input.Text;
            var match = TokenRegex.Match(text, start);

            while (match.Success)
            {
                switch (match.Value[0])
                {
                    // Comments
                    case '-':
                    case '/':
                        // Comments are verbatim
                        break;

                    // Quoted
                    case '\'':
                    case '[':
                        // Variable expansion requires switch to builder mode
                        if (HasVariableReplacement(match.Value))
                            return BuildNextBatch(input, start, match);

                        // Other quoted strings/identifiers are verbatim
                        break;

                    // Preprocessor directives
                    case '$':
                    case ':':
                        // Requires switch to builder mode
                        return BuildNextBatch(input, start, match);

                    // Batch separator
                    case 'g':
                    case 'G':
                        // Entire batch is verbatim => return portion of original input
                        return (
                            sql:  text.Substring(start, match.Index - start),
                            next: match.Index + match.Length
                        );
                }

                match = match.NextMatch();
            }

            return ( text.Substring(start), text.Length );
        }

        private (string sql, int next) BuildNextBatch(Input input, int start, Match match)
        {
            var text    = input.Text;
            var builder = InitializeBuilder(text, start, match.Index);

            do
            {
                switch (match.Value[0])
                {
                    // Comments
                    case '-':
                    case '/':
                        // Comments are verbatim
                        builder.Append(match.Value);
                        break;

                    // Quoted
                    case '\'':
                    case '[':
                        // Variable expansion requires switch to builder mode
                        if (HasVariableReplacement(match.Value))
                            return BuildNextBatch(input, start, match);

                        // Other quoted strings/identifiers are verbatim
                        builder.Append(match.Value);
                        break;

                    // Variable expansion
                    case '$':
                        break;

                    // Preprocessor directive
                    case ':':
                        break;

                    // Batch separator
                    case 'g':
                    case 'G':
                        // Finish batch
                        return (
                            sql:  builder.ToString(),
                            next: match.Index + match.Length
                        );
                }

                match = match.NextMatch();
            }
            while (match.Success);

            if (false)
                builder.Append("");

            return (
                sql:  builder.ToString(),
                next: text.Length
            );
        }

        private void PerformVariableReplacement(string text)
        {
            var builder = _builder;
            var start   = 0;
            var length  = text.Length;

            while (start < length)
            {
                var match = VariableRegex.Match(text, start);

                if (!match.Success)
                {
                    builder.Append(text, start, length - start);
                    return;
                }

                builder.Append(text, start, match.Index);

                var name = match.Groups["name"].Value;

                if (!_variables.TryGetValue(name, out var value))
                    throw new Exception();

                builder.Append(value);

                start = match.Index + match.Length;
            }
        }

        private StringBuilder InitializeBuilder(string text, int start, int end)
        {
            const int MinimumBufferSize = 4096;

            // Calculate sizes
            var length   = end - start;
            var capacity = length < MinimumBufferSize
                ? MinimumBufferSize
                : GetNextPowerOf2Saturating(length);

            var builder = _builder;
            if (builder == null)
            {
                // Create builder for first time
                 builder = new StringBuilder(text, start, length, capacity);
                _builder = builder;
            }
            else // (builder != null)
            {
                // Reuse builder
                builder.Clear();
                builder.EnsureCapacity(capacity);
                builder.Append(text, start, length);
            }

            return builder;
        }

        private static int GetNextPowerOf2Saturating(int value)
        {
            // https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            // but saturating at int.MaxValue instead of overflow

            value--;
            value |= value >>  1;
            value |= value >>  2;
            value |= value >>  4;
            value |= value >>  8;
            value |= value >> 16;

            return value == int.MaxValue
                ? value         // edge case: avoid overflow
                : value + 1;    // normal
        }

        private static bool HasVariableReplacement(string text)
        {
            return VariableRegex.IsMatch(text);
        }

        private static readonly Regex TokenRegex = new Regex(
            @"
                '    ( [^']  | ''   )*  ( '     | \z ) |     # string
                \[   ( [^\]] | \]\] )*  ( \]    | \z ) |     # quoted identifier
                --   .*?                ( \r?\n | \z ) |     # line comment
                /\*  ( .     | \n   )*? ( \*/   | \z ) |     # block comment
                \$\( (?<name>\w+)       ( \)    | \z ) |     # variable replacement
                ^:r      \s+            ( \r?\n | \z ) |     # include directive
                ^:setvar \s+            ( \r?\n | \z ) |     # set-variable directive
                ^GO                     ( \r?\n | \z )       # batch separator
            ",
            Options
        );

        private static readonly Regex VariableRegex = new Regex(
            @"
                \$\( (?<name>\w+) \)
            ",
            Options
        );

        private const RegexOptions Options
            = Multiline
            | IgnoreCase
            | CultureInvariant
            | IgnorePatternWhitespace
            | ExplicitCapture
            | Compiled;

        private class Input
        {
            public Input(string text, string name, Input parent = null)
            {
                Text   = text ?? throw new ArgumentNullException(nameof(text));
                Name   = name ?? throw new ArgumentNullException(nameof(name));
                Parent = parent;
            }

            public string Text   { get; }
            public string Name   { get; }
            public Input  Parent { get; }
        }
    }
}
