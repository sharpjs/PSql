using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

namespace PSql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SqlCmdPreprocessorTests
    {
        [Test]
        public void Variables_Initial()
        {
            new SqlCmdPreprocessor()
                .Variables.Should().BeEmpty();
        }

        [Test]
        public void Process_NullText()
        {
            new SqlCmdPreprocessor()
                .Invoking(p => p.Process(null))
                .Should().Throw<ArgumentNullException>()
                .Where(e => e.ParamName == "text");
        }

        [Test]
        public void Process_EmptyText()
        {
            var preprocessor = new SqlCmdPreprocessor();

            var batches = preprocessor.Process("");

            batches.Should().BeEmpty();
        }

        [Test]
        public void Process_TokenlessText()
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = "no tokens here";

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_LineComment(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a", "b -- foo", "c");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BlockComment(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a /* foo", "bar */ b");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BlockComment_Unterminated(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a /* foo");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_SingleQuotedString(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a 'foo''bar", "baz' b");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_SingleQuotedString_Unterminated(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a 'foo''bar");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BracketQuotedIdentifier(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a [foo]]bar", "baz] b");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BracketQuotedIdentifier_Unterminated(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            var text = Lines(eol, eof, "a [foo]]bar");

            var batches = preprocessor.Process(text);

            batches.Should().Equal(text);
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BatchSeparator(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            foreach (var separator in BatchSeparators)
            {
                var text = Lines(eol, eof, "a", separator, "b");

                var batches = preprocessor.Process(text);

                batches.Should().Equal("a" + eol, "b" + eof);
            }
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BatchSeparator_Repeated(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            foreach (var separator in BatchSeparators)
            {
                var text = Lines(eol, eof, "a", separator, separator, "b");

                var batches = preprocessor.Process(text);

                batches.Should().Equal("a" + eol, "b" + eof);
            }
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BatchSeparator_EmptyBatchAtEnd(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            foreach (var separator in BatchSeparators)
            {
                var text = Lines(eol, eof, "a", separator);

                var batches = preprocessor.Process(text);

                batches.Should().Equal("a" + eol);
            }
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_BatchSeparator_AllEmptyBatches(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            foreach (var separator in BatchSeparators)
            {
                var text = Lines(eol, eof, separator, separator);

                var batches = preprocessor.Process(text);

                batches.Should().BeEmpty();
            }
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_VariableReplacement_Normal(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["foo"] = "bar" }
            };

            var batches = preprocessor.Process(
                Lines(eol, eof, "a $(foo) b")
            );

            batches.Should().Equal(
                Lines(eol, eof, "a bar b")
            );
        }

        [Test]
        public void Process_VariableReplacement_Undefined()
        {
            var preprocessor = new SqlCmdPreprocessor { };

            preprocessor
                .Invoking(p => p.Process("$(foo)").Count())
                .Should().Throw<SqlCmdException>()
                .WithMessage("SqlCmd variable 'foo' is not defined.");
        }

        [Test]
        public void Process_VariableReplacement_Unterminated()
        {
            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["foo"] = "bar" }
            };

            preprocessor
                .Invoking(p => p.Process("$(foo").Count())
                .Should().Throw<SqlCmdException>()
                .WithMessage("Unterminated reference to SqlCmd variable 'foo'.");
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_VariableReplacement_InSingleQuotedString(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["foo"] = "bar" }
            };

            var batches = preprocessor.Process(
                Lines(eol, eof, "a 'b '' $(foo) c' d")
            );

            batches.Should().Equal(
                Lines(eol, eof, "a 'b '' bar c' d")
            );
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_VariableReplacement_InBracketQuotedIdentifier(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["foo"] = "bar" }
            };

            var batches = preprocessor.Process(
                Lines(eol, eof, "a [b ]] $(foo) c] d")
            );

            batches.Should().Equal(
                Lines(eol, eof, "a [b ]] bar c] d")
            );
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_SetVariable_Unset(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["foo"] = "bar" }
            };

            var text = Lines(eol, eof, ":setvar foo", "$(foo)");

            preprocessor
                .Invoking(p => p.Process(text).Count())
                .Should().Throw<SqlCmdException>()
                .WithMessage("SqlCmd variable 'foo' is not defined.");

            preprocessor.Variables.Should().NotContainKey("foo");
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_SetVariable_Unquoted(string eol, string eof)
        {
            // NOTE: This test contains German, Japanese, and Russian characters.

            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["f0ö_Бар-baß"] = "original value" }
            };

            var batches = preprocessor.Process(
                Lines(eol, eof,
                    @"a",
                    @":setvar f0ö_Бар-baß qux~`!@#$%^&*()-_=+[{]}\|;:',<.>/?ほげ",
                    @"b $(f0ö_Бар-baß) c"
                )
            );

            batches.Should().Equal(
                Lines(eol, eof,
                    @"a",
                    @"b qux~`!@#$%^&*()-_=+[{]}\|;:',<.>/?ほげ c"
                )
            );

            preprocessor.Variables["f0ö_Бар-baß"].Should().Be(
                @"qux~`!@#$%^&*()-_=+[{]}\|;:',<.>/?ほげ"
            );
        }

        [Test]
        [TestCaseSource(nameof(EolEofCases))]
        public void Process_SetVariable_Quoted(string eol, string eof)
        {
            // NOTE: This test contains German, Japanese, and Russian characters.

            var preprocessor = new SqlCmdPreprocessor
            {
                Variables = { ["f0ö_Бар-baß"] = "original value" }
            };

            var batches = preprocessor.Process(
                Lines(eol, eof,
                    @"a",
                    @":setvar f0ö_Бар-baß ""qux ~`!@#$%^&*()-_=+[{]}\|;:'"""",<.>/? corge ",
                    @"",
                    @"「ほげ」 grault""",
                    @"b $(f0ö_Бар-baß) c"
                )
            );

            batches.Should().Equal(
                Lines(eol, eof,
                    @"a",
                    @"b qux ~`!@#$%^&*()-_=+[{]}\|;:'"",<.>/? corge ",
                    @"",
                    @"「ほげ」 grault c"
                )
            );

            preprocessor.Variables["f0ö_Бар-baß"].Should().Be(
                string.Concat(
                    @"qux ~`!@#$%^&*()-_=+[{]}\|;:'"",<.>/? corge ", eol,
                    @"",                                             eol,
                    @"「ほげ」 grault"
                )
            );
        }

        private static readonly string[][] EolEofCases =
        {
            //         EOL     EOF
            new[] {   "\n",     "" },
            new[] { "\r\n",     "" },
            new[] {   "\n",   "\n" },
            new[] { "\r\n", "\r\n" }
        };

        private static readonly string[] BatchSeparators =
        {
            "GO", "Go", "gO", "go"
        };

        private static string Lines(string eol, string eof, params string[] lines)
        {
            var text = new StringBuilder(capacity: 64);

            if (lines.Length > 0)
                text.Append(lines[0]);

            for (var i = 1; i < lines.Length; i++)
                text.Append(eol).Append(lines[i]);

            return text.Append(eof).ToString();
        }
    }
}
