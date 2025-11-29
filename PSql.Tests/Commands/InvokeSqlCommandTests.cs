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

    [Test]
    public void StopProcessing()
    {
        var command = new TestCommand { Sql = ["--"] };

        command.BeginProcessing();
        command.StopProcessing();

        Should.Throw<OperationCanceledException>(
            () => command.ProcessRecord()
        );
    }

    private class TestCommand : InvokeSqlCommand
    {
        public new void BeginProcessing() => base.BeginProcessing();
        public new void ProcessRecord()   => base.ProcessRecord();
        public new void StopProcessing()  => base.StopProcessing();
    }
}
