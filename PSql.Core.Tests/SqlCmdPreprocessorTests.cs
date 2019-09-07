using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

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
        public void Process_BatchSeparator_AtEof(string eol, string eof)
        {
            var preprocessor = new SqlCmdPreprocessor();

            foreach (var separator in BatchSeparators)
            {
                var text = Lines(eol, eof, "a", separator);

                var batches = preprocessor.Process(text);

                batches.Should().Equal("a" + eol);
            }
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
