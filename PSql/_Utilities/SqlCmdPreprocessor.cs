using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PSql
{
    using static RegexOptions;

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

            var input = new Input(name ?? DefaultInputName, text);
            return ProcessCore(input);
        }

        private IEnumerable<string> ProcessCore(Input input)
        {
            string batch;

            do
            {
                (batch, input) = GetNextBatch(input);

                if (batch.Length != 0)
                    yield return batch;
            }
            while (input != null);
        }

        // Verbatim mode - reuse input string if possible
        private (string, Input) GetNextBatch(Input input)
        {
            var start = input.Index;

            for (;;)
            {
                var match = input.NextToken();

                // Handle end of input
                if (!match.Success)
                {
                    // End of top-level input => final batch
                    if (input.Parent == null)
                        return (input.Range(start), null);

                    // Non-empty batch continuing in parent input => switch to builder mode
                    if (start != input.Length)
                        return BuildNextBatch(input, start, match);

                    // Empty batch continuing in the parent input => continue in verbatim mode
                    input = input.Parent;
                    start = input.Index;
                    continue;
                }

                // Handle token found
                switch (match.Value[0])
                {
                    // Comments
                    default:
                    case '-':
                    case '/':
                        // Comments are verbatim
                        continue;

                    // Quoted
                    case '\'':
                    case '[':
                        // Variable expansion requires switch to builder mode
                        if (HasVariableReplacement(match.Value))
                            return BuildNextBatch(input, start, match);

                        // Other quoted strings/identifiers are verbatim
                        continue;

                    // Preprocessor directives
                    case '$':
                    case ':':
                        // Requires switch to builder mode
                        return BuildNextBatch(input, start, match);

                    // Batch separator
                    case 'g':
                    case 'G':
                        // Entire batch is verbatim => return portion of original input
                        return (input.Range(start, match.Index), input);
                }
            }
        }

        // Builder mode - assemble batch in a StringBuilder
        private (string, Input) BuildNextBatch(Input input, int start, Match match)
        {
            var builder = InitializeBuilder(start, match.Index, input.Length);

            for (;;)
            {
                // Handle end of input
                if (!match.Success)
                {
                    input.AppendRangeTo(builder, start);

                    // End of top-level input => final batch
                    if (input.Parent == null)
                        return (builder.ToString(), null);

                    // Batch continues in parent input
                    input = input.Parent;
                    start = input.Index;
                    match = input.NextToken();
                    continue;
                }

                input.AppendRangeTo(builder, start, match.Index);

                // Handle token found
                switch (match.Value[0])
                {
                    // Comments
                    default:
                    case '-':
                    case '/':
                        // Comments are verbatim
                        builder.Append(match.Value);
                        break;

                    // Quoted
                    case '\'':
                    case '[':
                        // Quoted strings/identifiers are subject to variable replacement
                        PerformVariableReplacement(match.Value);
                        break;

                    // Variable expansion
                    case '$':
                        builder.Append(GetVariableReplacement(match));
                        break;

                    // Preprocessor directive
                    case ':':
                        var args = match.Groups["args"].Value;
                        if (match.Value[1] == 'r')
                            Include(args, ref input);
                        else // :setvar
                            SetVariable(args);
                        break;

                    // Batch separator
                    case 'g':
                    case 'G':
                        // Finish batch
                        return (builder.ToString(), input);
                }

                start = input.Index;
                match = input.NextToken();
            }
        }

        private void Include(string args, ref Input input)
        {
            var match = IncludeRegex.Match(args);
            if (!match.Success)
                throw new SqlCmdException("Invalid syntax in :r directive.");

            string path;
            path = match.Groups[nameof(path)].Value;
            path = Unquote(path);

            var text = IoHelper.ReadText(path);

            input = new Input(path, text, input);
        }

        private void SetVariable(string args)
        {
            var match = SetVariableRegex.Match(args);
            if (!match.Success)
                throw new SqlCmdException("Invalid syntax in :setvar directive.");

            string name, value;
            name  = match.Groups[nameof(name )].Value;
            value = match.Groups[nameof(value)].Value;

            if (string.IsNullOrEmpty(value))
                _variables.Remove(name);
            else
                _variables[name] = Unquote(value);
        }

        internal string Unquote(string value)
        {
            const char
                Quote = '"';

            const string
                QuoteUnescaped = @"""",
                QuoteEscaped   = QuoteUnescaped + QuoteUnescaped;

            if (value.Length == 0 || value[0] != Quote)
                return value;

            if (value.Length == 1 || value[value.Length - 1] != Quote)
                throw new SqlCmdException("Unterminated double-quoted string.");

            return value
                .Substring(1, value.Length - 2)
                .Replace(QuoteEscaped, QuoteUnescaped);
        }

        private static bool HasVariableReplacement(string text)
        {
            return VariableRegex.IsMatch(text);
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
                builder.Append(GetVariableReplacement(match));

                start = match.Index + match.Length;
            }
        }

        private string GetVariableReplacement(Match match)
        {
            var name = match.Groups["name"];

            var unterminated = match.Index + match.Length == name.Index + name.Length;
            if (unterminated)
                throw new SqlCmdException($"Unterminated reference to SqlCmd variable '{name.Value}'.");

            if (!_variables.TryGetValue(name.Value, out var value))
                throw new SqlCmdException($"SqlCmd variable '{name.Value}' is not defined.");

            return value;
        }

        private StringBuilder InitializeBuilder(int start, int end, int length)
        {
            const int MinimumBufferSize = 4096;

            // Calculate sizes
            length = (end > 0 ? end : length) - start;
            var capacity = length < MinimumBufferSize
                ? MinimumBufferSize
                : GetNextPowerOf2Saturating(length);

            var builder = _builder;
            if (builder == null)
            {
                // Create builder for first time
                _builder = builder = new StringBuilder(capacity);
            }
            else // (builder != null)
            {
                // Reuse builder
                builder.Clear();
                builder.EnsureCapacity(capacity);
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

        private class Input
        {
            public Input(string name, string text, Input parent = null)
            {
                Name   = name ?? throw new ArgumentNullException(nameof(name));
                Text   = text ?? throw new ArgumentNullException(nameof(text));
                Parent = parent;
            }

            public string Name   { get; }
            public string Text   { get; }
            public int    Index  { get; private set; }
            public Input  Parent { get; }

            public Match NextToken()
            {
                var match = TokenRegex.Match(Text, Index);

                Index = match.Index + match.Length;

                return match;
            }

            public int Length
                => Text.Length;

            public string Range(int start)
                => Text.Substring(start);

            public string Range(int start, int end)
                => Text.Substring(start, end - start);

            public void AppendRangeTo(StringBuilder builder, int start)
                => builder.Append(Text, start, Text.Length - start);

            public void AppendRangeTo(StringBuilder builder, int start, int end)
                => builder.Append(Text, start, end - start);
        }

        private static readonly Regex TokenRegex = new Regex(
            @"
                '    ( [^']  | ''   )*      ( '     | \z ) |    # string
                \[   ( [^\]] | \]\] )*      ( \]    | \z ) |    # quoted identifier
                --   .*?                    ( \r?\n | \z ) |    # line comment
                /\*  ( .     | \n   )*?     ( \*/   | \z ) |    # block comment
                \$\( (?<name> [\w-]+ )      ( \)    | \z ) |    # variable replacement
                ^GO                         ( \r?\n | \z ) |    # batch separator
                ^:(r|setvar) (?<args>                           # directives
                    ( [^""\r\n]
                    | \r (?!\n)
                    | "" ( [^""] | """" )*  ( ""    | \z )
                    )*
                )                           ( \r?\n | \z )
            ",
            Options
        );

        private static readonly Regex VariableRegex = new Regex(
            @"
                \$\( (?<name>\w+) \)
            ",
            Options
        );

        private static readonly Regex IncludeRegex = new Regex(
            @"
                \A [ \t]+

                (?<path> [^"" \t\r\n]+                      # unquoted
                |        "" ( [^""] | """" )* ( "" | \z )   # quoted
                )

                [ \t]* \z
            ",
            Options
        );

        private static readonly Regex SetVariableRegex = new Regex(
            @"
                \A [ \t]+

                (?<name> [\w-[0-9]] [\w-]* )

                (?> [ \t]+
                    (?<value> [^"" \t\r\n]+                     # unquoted
                    |         "" ( [^""] | """" )* ( "" | \z )  # quoted
                    )
                )?

                [ \t]* \z
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

        private const string
            DefaultInputName = "(script)";
    }
}
