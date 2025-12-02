// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace PSql.Internal;

/// <summary>
///   A dispatcher that executes dispatched actions on the main thread.
/// </summary>
internal sealed class MainThreadDispatcher : IDispatcher, IDisposable
{
    private readonly BlockingCollection<Action> _queue;
    private readonly int                        _mainThreadId;

    /// <summary>
    ///   Initializes a new <see cref="MainThreadDispatcher"/> instance with
    ///   the current thread as the main thread.
    /// </summary>
    public MainThreadDispatcher()
    {
        _queue        = [];
        _mainThreadId = CurrentThreadId;
    }

    private static int CurrentThreadId => Environment.CurrentManagedThreadId;

    /// <summary>
    ///   Dispatches the specified action to the main thread.
    /// </summary>
    /// <param name="action">
    ///   The action to dispatch.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     If invoked from the main thread (the thread that constructed the
    ///     <see cref="MainThreadDispatcher"/> instance), this method executes
    ///     the <paramref name="action"/> immediately and synchronously.
    ///     Otherwise, this method queues the <paramref name="action"/> for
    ///     execution on the main thread by <see cref="Run"/> or
    ///     <see cref="RunPending"/>, then returns <b>without waiting</b> for
    ///     the action to complete.
    ///   </para>
    ///   <para>
    ///     This method is thread-safe.
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   The <see cref="MainThreadDispatcher"/> is in the completed state.
    ///   No further actions may be dispatched.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="MainThreadDispatcher"/> has been disposed.
    /// </exception>
    public void Post(Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (CurrentThreadId == _mainThreadId)
            action();
        else
            _queue.Add(action);
    }

    /// <summary>
    ///   Invokes any pending actions queued by <see cref="Post"/>.
    /// </summary>
    /// <remarks>
    ///   This method <b>must</b> be invoked on the main thread (the thread
    ///   that constructed the <see cref="MainThreadDispatcher"/> instance).
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   This method was invoked on a thread other than the thread that
    ///   constructed the <see cref="MainThreadDispatcher"/> instance.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="MainThreadDispatcher"/> has been disposed.
    /// </exception>
    public void RunPending()
    {
        if (CurrentThreadId != _mainThreadId)
            throw OnInvokedFromNonMainThread();

        while (_queue.TryTake(out var action))
            action();
    }

    /// <summary>
    ///   Invokes actions queued by <see cref="Post"/> continuously until
    ///   <see cref="Complete"/> is called.
    /// </summary>
    /// <remarks>
    ///   This method <b>must</b> be invoked on the main thread (the thread
    ///   that constructed the <see cref="MainThreadDispatcher"/> instance).
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   This method was invoked on a thread other than the thread that
    ///   constructed the <see cref="MainThreadDispatcher"/> instance.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="MainThreadDispatcher"/> has been disposed.
    /// </exception>
    public void Run()
    {
        if (CurrentThreadId != _mainThreadId)
            throw OnInvokedFromNonMainThread();

        while (_queue.TryTake(out var action, Timeout.Infinite))
            action();
    }

    /// <summary>
    ///   Transitions the <see cref="MainThreadDispatcher"/> to the completed
    ///   state.
    /// </summary>
    /// <remarks>
    ///   In the completed state, <see cref="Post"/> does not accept any
    ///   further actions, and <see cref="Run"/> returns after executing any
    ///   remaining queued actions.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="MainThreadDispatcher"/> has been disposed.
    /// </exception>
    public void Complete()
    {
        _queue.CompleteAdding();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _queue.Dispose();
    }

    private static Exception OnInvokedFromNonMainThread()
    {
        return new InvalidOperationException(
            "This method must be invoked from the thread that constructed the dispatcher."
        );
    }
}
