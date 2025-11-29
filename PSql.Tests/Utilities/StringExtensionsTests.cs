// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    [TestCase(null, false)]
    [TestCase("",   false)]
    [TestCase("a",  true )]
    public void HasContent(string? s, bool expected)
    {
        s.HasContent().ShouldBe(expected);
    }

    [Test]
    [TestCase(null, true )]
    [TestCase("",   true )]
    [TestCase("a",  false)]
    public void IsNullOrEmpty(string? s, bool expected)
    {
        s.IsNullOrEmpty().ShouldBe(expected);
    }

    [Test]
    [TestCase(null, null)]
    [TestCase("",   null)]
    [TestCase("a",  "a" )]
    public void NullIfEmpty(string? s, string? expected)
    {
        s.NullIfEmpty().ShouldBe(expected);
    }

    [Test]
    [TestCase(null, null, null)]
    [TestCase("",   null, ""  )]
    [TestCase("a",  null, "a" )]
    [TestCase(null, "",   null)]
    [TestCase("",   "",   null)]
    [TestCase("a",  "",   "a" )]
    [TestCase(null, "a",  null)]
    [TestCase("",   "a",  ""  )]
    [TestCase("a",  "a",  null)]
    public void NullIf(string? s, string? nullish, string? expected)
    {
        s.NullIf(nullish).ShouldBe(expected);
    }
}
