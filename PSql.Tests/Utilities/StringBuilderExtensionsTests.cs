// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Text;

namespace PSql.Tests.Unit;

[TestFixture]
public class StringBuilderExtensionsTests
{
    [Test]
    [TestCase("",    "''"    )]
    [TestCase("a",   "'a'"   )]
    [TestCase("'",   "''''"  )]
    [TestCase("a'b", "'a''b'")]
    public void AppendQuoted(string input, string output)
    {
        var builder = new StringBuilder();

        var returned = builder.AppendQuoted(input, '\'');

        returned.ShouldBeSameAs(builder);

        builder.ToString().ShouldBe(output);
    }

    [Test]
    public void AppendQuoted_NullBuilder()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            (null as StringBuilder)!.AppendQuoted("any", '\'');
        });
    }

    [Test]
    [TestCase(@"",    @""    )]
    [TestCase(@"a",   @"a"   )]
    [TestCase(@"\",   @"\\"  )]
    [TestCase(@"a\b", @"a\\b")]
    public void AppendEscaped(string input, string output)
    {
        var builder = new StringBuilder();

        var returned = builder.AppendEscaped(input, '\\');

        returned.ShouldBeSameAs(builder);

        builder.ToString().ShouldBe(output);
    }

    [Test]
    public void AppendEscaped_NullBuilder()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            (null as StringBuilder)!.AppendEscaped("any", '\\');
        });
    }
}
