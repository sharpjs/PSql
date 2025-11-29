// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

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
