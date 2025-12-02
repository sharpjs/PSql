// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Internal;

[TestFixture]
public class MainThreadDispatcherTests
{
    [Test]
    public void Post_Null()
    {
        using var dispatcher = new MainThreadDispatcher();

        Should.Throw<ArgumentNullException>(() =>
        {
            dispatcher.Post(null!);
        });
    }

    [Test]
    public void Post_WithoutRun()
    {
        using var dispatcher = new MainThreadDispatcher();

        var mainThreadId = 0;

        dispatcher.Post(() => mainThreadId = CurrentThreadId);

        mainThreadId.ShouldBe(CurrentThreadId);
    }

    [Test]
    public void Post_DuringRun()
    {
        using var dispatcher = new MainThreadDispatcher();

        var mainThreadId = 0;
        var taskThreadId = 0;

        var task = Task.Run(() =>
        {
            taskThreadId = CurrentThreadId;
            dispatcher.Post(() => mainThreadId = CurrentThreadId);
            dispatcher.Complete();
        });

        dispatcher.Run();
        task.Wait();

        mainThreadId.ShouldBe(CurrentThreadId);
        taskThreadId.ShouldNotBe(CurrentThreadId);
        taskThreadId.ShouldNotBe(0);
    }

    [Test]
    public void Post_DuringRun_Nested()
    {
        using var dispatcher = new MainThreadDispatcher();

        var mainThreadId = 0;
        var taskThreadId = 0;

        var task = Task.Run(() =>
        {
            taskThreadId = CurrentThreadId;
            dispatcher.Post(() => dispatcher.Post(() => mainThreadId = CurrentThreadId));
            dispatcher.Complete();
        });

        dispatcher.Run();
        task.Wait();

        mainThreadId.ShouldBe(CurrentThreadId);
        taskThreadId.ShouldNotBe(CurrentThreadId);
        taskThreadId.ShouldNotBe(0);
    }

    [Test]
    public void Post_Completed_OnMainThread()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Complete();

        var invoked = false;

        dispatcher.Post(() => invoked = true);

        invoked.ShouldBeTrue();
    }

    [Test]
    public void Post_Completed_OnOtherThread()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Complete();

        var invoked = false;

        Task.Run(() =>
        {
            Should.Throw<InvalidOperationException>(() =>
            {
                dispatcher.Post(() => invoked = true);
            });
        })
        .GetAwaiter().GetResult();

        invoked.ShouldBeFalse();
    }

    [Test]
    public void Post_Disposed_OnMainThread()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Dispose();

        var invoked = false;

        dispatcher.Post(() => invoked = true);

        invoked.ShouldBeTrue();
    }

    [Test]
    public void Post_Disposed_OnOtherThread()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Dispose();

        var invoked = false;

        Task.Run(() =>
        {
            Should.Throw<ObjectDisposedException>(() =>
            {
                dispatcher.Post(() => invoked = true);
            });
        })
        .GetAwaiter().GetResult();

        invoked.ShouldBeFalse();
    }

    [Test]
    public void RunPending_OnOtherThread()
    {
        using var dispatcher = new MainThreadDispatcher();

        Should.Throw<InvalidOperationException>(() =>
        {
            Task.Run(() => dispatcher.RunPending()).GetAwaiter().GetResult();
        })
        .Message.ShouldBe("This method must be invoked from the thread that constructed the dispatcher.");
    }

    [Test]
    public void RunPending_Completed()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Complete();

        dispatcher.RunPending();
    }

    [Test]
    public void RunPending_Disposed()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Dispose();

        Should.Throw<ObjectDisposedException>(dispatcher.RunPending);
    }

    [Test]
    public void RunPending_Ok()
    {
        using var dispatcher = new MainThreadDispatcher();

        var invoked = false;

        Task.Run(() => dispatcher.Post(() => invoked = true)).GetAwaiter().GetResult();

        dispatcher.RunPending();

        invoked.ShouldBeTrue();
    }

    [Test]
    public void Run_OnOtherThread()
    {
        using var dispatcher = new MainThreadDispatcher();

        Should.Throw<InvalidOperationException>(() =>
        {
            Task.Run(() => dispatcher.Run()).GetAwaiter().GetResult();
        })
        .Message.ShouldBe("This method must be invoked from the thread that constructed the dispatcher.");
    }

    [Test]
    public void Run_Completed()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Complete();

        dispatcher.Run();
    }

    [Test]
    public void Run_Disposed()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Dispose();

        Should.Throw<ObjectDisposedException>(dispatcher.Run);
    }

    [Test]
    public void Complete_Disposed()
    {
        using var dispatcher = new MainThreadDispatcher();

        dispatcher.Dispose();

        Should.Throw<ObjectDisposedException>(dispatcher.Complete);
    }

    private static int CurrentThreadId => Thread.CurrentThread.ManagedThreadId;
}
