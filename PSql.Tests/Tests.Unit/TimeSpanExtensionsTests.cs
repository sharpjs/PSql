// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Tests.Unit;

[TestFixture]
public class TimeSpanExtensionsTests
{
    [Test]
    [TestCase(      "00:00:00.000", 0)]
    [TestCase(      "00:00:09.876", 9)]
    [TestCase(     "-00:00:09.876", 9)]
    [TestCase("24855.03:14:07.000", int.MaxValue)]
    [TestCase("24855.23:59:59.999", int.MaxValue)]
    public void GetAbsoluteSecondsSaturatingInt32(TimeSpan input, int output)
    {
        input.GetAbsoluteSecondsSaturatingInt32().ShouldBe(output);
    }
}
