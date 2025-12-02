// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace PSql.Internal;

[TestFixture]
public class DispatcherExtensionsTests
{
    private const IDispatcher?     NullDispatcher = null;
    private const Action<int>?     NullAction     = null;
    private const Func<int, bool>? NullFunc       = null;
    private const long             DelayTicks     = 50 * TimeSpan.TicksPerMillisecond;

    private static TimeSpan Delay => TimeSpan.FromTicks(DelayTicks);

    [Test]
    public void Invoke_Void_NullDispatcher()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            NullDispatcher!.Invoke(arg => { }, 42);
        });
    }

    [Test]
    public void Invoke_Void_NullAction()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            Mock.Of<IDispatcher>().Invoke(NullAction!, 42);
        });
    }

    [Test]
    public void Invoke_Void()
    {
        var dispatcher = new DelayedDispatcher();
        var value      = 0;
        var stopwatch  = Stopwatch.StartNew();

        dispatcher.Invoke(arg => { value = arg; }, arg: 42);

        stopwatch.Stop();
        value            .ShouldBe(42);
        stopwatch.Elapsed.ShouldBeGreaterThan(Delay);
    }

    [Test]
    public void Invoke_Result_NullDispatcher()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            NullDispatcher!.Invoke(arg => arg, 42);
        });
    }

    [Test]
    public void Invoke_Result_NullAction()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            Mock.Of<IDispatcher>().Invoke(NullFunc!, 42);
        });
    }

    [Test]
    public void Invoke_Result()
    {
        var dispatcher = new DelayedDispatcher();
        var stopwatch  = Stopwatch.StartNew();

        var result = dispatcher.Invoke(arg => arg, arg: 42);

        stopwatch.Stop();
        result           .ShouldBe(42);
        stopwatch.Elapsed.ShouldBeGreaterThan(Delay);
    }

    private class DelayedDispatcher : IDispatcher
    {
        public void Post(Action action)
        {
            Thread.Sleep(Delay);
            action();
        }
    }
}
