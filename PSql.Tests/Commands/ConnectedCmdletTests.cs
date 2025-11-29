// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

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

    [Test]
    public void AssumeBeginProcessingInvoked_NotInvoked()
    {
        var cmdlet = new TestCmdletA();

#if DEBUG
        Should.Throw<InvalidOperationException>(
            () => cmdlet.AssumeBeginProcessingInvoked()
        );
#else
    // No-op in release builds
    cmdlet.AssumeBeginProcessingInvoked();
#endif
    }

    private class TestCmdletA : ConnectedCmdlet
    {
        public new void AssumeBeginProcessingInvoked()
        {
            base.AssumeBeginProcessingInvoked();
        }
    }

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
