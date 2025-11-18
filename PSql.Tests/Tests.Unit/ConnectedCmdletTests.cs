// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Commands;

namespace PSql.Tests;

[TestFixture]
public class ConnectedCmdletTests
{
    [Test]
    public void Dispose_NotInvoked()
    {
        var cmdlet = new TestCmdletA();

        cmdlet.Dispose();
        cmdlet.Dispose(); // To test multiple disposal
    }

    [Test]
    public void Dispose_ConnectionNullified()
    {
        var cmdlet = new TestCmdletB();

        cmdlet.Dispose();
        cmdlet.Dispose(); // To test multiple disposal
    }

    private class TestCmdletA : ConnectedCmdlet { }

    private class TestCmdletB : ConnectedCmdlet
    {
        // This class does several things wrongly in order to test an edge
        // case: where _ownsConnection is true but Connection is null.

        public TestCmdletB()
        {
            // This here to set _ownsConnection to true
            Context = new();
            BeginProcessing();
        }

        public override void Dispose()
        {
            // This here to set Connection to null
            Connection?.Dispose();
            Connection = null;

            base.Dispose();
        }
    }
}
