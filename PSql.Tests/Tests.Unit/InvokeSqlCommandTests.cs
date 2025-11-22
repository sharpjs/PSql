// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Commands;

namespace PSql.Tests.Unit;

[TestFixture]
public class InvokeSqlCommandTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    public void ProcessRecord_NullSql()
    {
        // PowerShell parameter validation should prevent this case, but the
        // code should still handle it gracefully
        new TestCommand().ProcessRecord();
    }

    private class TestCommand : InvokeSqlCommand
    {
        public new void ProcessRecord() => base.ProcessRecord();
    }
}
