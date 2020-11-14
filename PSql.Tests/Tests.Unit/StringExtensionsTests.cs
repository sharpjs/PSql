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
using FluentAssertions;
using NUnit.Framework;

namespace PSql.Tests.Unit
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
