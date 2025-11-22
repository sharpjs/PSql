// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Commands;

namespace PSql.Tests.Unit;

[TestFixture]
public class PSqlCmdletTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    public void Foo()
    {
        Should.Throw<NotImplementedException>(() =>
        {
            new TestCmdlet().WriteHost(null);
        })
        .Message.ShouldContain("WriteInformation");
    }

    private class TestCmdlet : PSqlCmdlet { }
}
