// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Internal;

namespace PSql.Commands;

[TestFixture]
public class AsyncCmdletScopeTests
{
    [Test]
    public void Construct_WithAmbientSynchronizationContext()
    {
        var previousContext = SynchronizationContext.Current;

        try
        {
            var ambientContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(ambientContext);

            using (var scope = new AsyncCmdletScope())
                SynchronizationContext.Current.ShouldBeNull();

            SynchronizationContext.Current.ShouldBe(ambientContext);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
        }
    }

    [Test]
    public void Dispatcher_Get()
    {
        using var scope = new AsyncCmdletScope();

        scope.Dispatcher.ShouldBeOfType<MainThreadDispatcher>();
    }

    [Test]
    public void CancellationToken_Get()
    {
        using var cancellation = new CancellationTokenSource();
        using var scope        = new AsyncCmdletScope(cancellation.Token);

        scope.CancellationToken.ShouldBe(cancellation.Token);
    }

    [Test]
    public void Run_NullAction()
    {
        using var scope = new AsyncCmdletScope();

        Should.Throw<ArgumentNullException>(() => scope.Run(null!));
    }

    [Test]
    public void Complete_Canceled()
    {
        using var cancellation = new CancellationTokenSource();
        using var scope        = new AsyncCmdletScope(cancellation.Token);

        scope.Run(() => Task.Delay(Timeout.Infinite, cancellation.Token));

        cancellation.CancelAfter(50); // ms

        scope.Complete();
    }
}
