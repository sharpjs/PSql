// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql.Tests.Unit;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    [TestCase(null, false)]
    [TestCase("",   false)]
    [TestCase(" ",  true )]
    [TestCase("a",  true )]
    public void HasContent(string? s, bool expected)
    {
        s.HasContent().Should().Be(expected);
    }

    [Test]
    [TestCase(null, null)]
    [TestCase("",   null)]
    [TestCase(" ",  " " )]
    [TestCase("a",  "a" )]
    public void NullIfEmpty(string? s, string? expected)
    {
        s.NullIfEmpty().Should().Be(expected);
    }
}
