// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using PSql.Internal;

namespace PSql.Commands;

/// <summary>
///   A scope in which a cmdlet can invoke asynchronous code.
/// </summary>
internal sealed class AsyncCmdletScope : IDisposable
{
    private readonly ConcurrentBag<Task>     _tasks;
    private readonly MainThreadDispatcher    _dispatcher;
    private readonly CancellationToken       _cancellation;
    private readonly SynchronizationContext? _previousContext;

    /// <summary>
    ///   Initializes a new <see cref="AsyncCmdletScope"/> instance with the
    ///   current thread as the main thread.
    /// </summary>
    /// <param name="cancellation">
    ///   A token to monitor for cancellation requests.
    /// </param>
    public AsyncCmdletScope(CancellationToken cancellation = default)
    {
        _tasks           = [];
        _dispatcher      = new MainThreadDispatcher();
        _cancellation    = cancellation;
        _previousContext = SynchronizationContext.Current;

        // Ensure no synchronization context with conflicting thread mobility ideas
        if (_previousContext is not null)
            SynchronizationContext.SetSynchronizationContext(null);
    }

    /// <summary>
    ///   Gets a dispatcher that forwards invocations to the main thread.
    /// </summary>
    public IDispatcher Dispatcher => _dispatcher;

    /// <summary>
    ///   Gets the token to monitor for cancellation requests.
    /// </summary>
    public CancellationToken CancellationToken => _cancellation;

    /// <summary>
    ///   Queues the specified action to run asynchronously on the thread pool.
    /// </summary>
    /// <param name="action">
    ///   The action to run asynchronously.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   This method is thread-safe.
    /// </remarks>
    public void Run(Func<Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        _tasks.Add(Task.Run(action, _cancellation));
    }

    /// <summary>
    ///   Invokes any pending actions dispatched to the main thread.
    /// </summary>
    /// <remarks>
    ///   This method <b>must</b> be invoked on the main thread (the thread
    ///   that constructed the <see cref="AsyncCmdletScope"/> instance).
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   This method was invoked on a thread other than the thread that
    ///   constructed the <see cref="AsyncCmdletScope"/> instance.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="AsyncCmdletScope"/> has been disposed.
    /// </exception>
    public void InvokePendingMainThreadActions()
    {
        _dispatcher.RunPending();
    }

    /// <summary>
    ///   Invokes pending actions dispatched to the main thread, continuously,
    ///   until all asynchronous actions queued by <see cref="Run"/> complete.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This method <b>must</b> be invoked on the main thread (the thread
    ///     that constructed the <see cref="AsyncCmdletScope"/> instance).
    ///   </para>
    ///   <para>
    ///     This method may be invoked only once per
    ///     <see cref="AsyncCmdletScope"/> instance.
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   This method was invoked on a thread other than the thread that
    ///   constructed the <see cref="AsyncCmdletScope"/> instance.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="AsyncCmdletScope"/> has been disposed.
    /// </exception>
    public void Complete()
    {
        InvokePendingMainThreadActions();

        var task = Task.WhenAll(_tasks);

        task.ContinueWith(
            _ => _dispatcher.Complete(),
            TaskContinuationOptions.ExecuteSynchronously
        );

        _dispatcher.Run();

        // Observe task exceptions
        try
        {
            task.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // Not an error
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Restore previous synchronization context
        if (_previousContext is not null)
            SynchronizationContext.SetSynchronizationContext(_previousContext);

        _dispatcher.Dispose();
    }
}
