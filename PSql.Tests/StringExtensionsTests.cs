using System;
using FluentAssertions;
using NUnit.Framework;

#nullable enable

namespace PSql.Testing
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void Unindent_Null()
        {
            (null as string)
                .Invoking(s => s!.Unindent())
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Unindent_Empty()
        {
            "".Unindent().Should().BeEmpty();
        }

        [Test]
        public void Unindent_Lf()
        {
            "\n".Unindent().Should().BeEmpty();
        }

        [Test]
        public void Unindent_CrLf()
        {
            "\r\n".Unindent().Should().BeEmpty();
        }

        [Test]
        public void Unindent_OneLine_NotIndented()
        {
            "a".Unindent().Should().Be("a");
        }

        [Test]
        public void Unindent_OneLine_Indented()
        {
            "\t a".Unindent().Should().Be("a");
        }

        [Test]
        public void Unindent_MultiLine_NotIndented()
        {
            ( "\r\n"
            + "a\r\n"
            + "  b\r\n"
            + "    c\r\n"
            )
            .Unindent().Should().Be
            ( "a\r\n"
            + "  b\r\n"
            + "    c\r\n"
            );
        }

        [Test]
        public void Unindent_MultiLine_Indented()
        {
            ( "\r\n"
            + "    \t    a\r\n"
            + "    \t      b\r\n"
            + "    \t        c\r\n"
            )
            .Unindent().Should().Be
            ( "a\r\n"
            + "  b\r\n"
            + "    c\r\n"
            );
        }

        [Test]
        public void Unindent_MultiLine_Indented_TrailingIndent()
        {
            ( "\r\n"
            + "    \t    a\r\n"
            + "    \t      b\r\n"
            + "    \t        c\r\n"
            + "    \t    "
            )
            .Unindent().Should().Be
            ( "a\r\n"
            + "  b\r\n"
            + "    c\r\n"
            );
        }
    }
}
