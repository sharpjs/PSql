using System;
using System.Collections.Generic;
using System.Linq;
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
        public void Process_SingleBatch()
        {
            var preprocessor = new SqlCmdPreprocessor();

            var batches = preprocessor.Process(
                @"this is a single batch"
            );

            batches.Should().Equal(
                @"this is a single batch"
            );
        }
    }
}
