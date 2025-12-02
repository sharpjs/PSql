// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Internal;

namespace PSql.Commands;

/// <summary>
///   Base class for PowerShell cmdlets that can execute asynchronous code.
/// </summary>
public abstract class AsyncPSCmdlet : PSCmdlet, ICmdlet, IDisposable
{
    private AsyncCmdletScope? _asyncScope;

    private readonly CancellationTokenSource _cancellation = new();

    /// <inheritdoc cref="AsyncCmdletScope.Dispatcher"/>
    public IDispatcher Dispatcher
        => _asyncScope?.Dispatcher ?? ImmediateDispatcher.Instance;

    /// <inheritdoc cref="AsyncCmdletScope.CancellationToken"/>
    public CancellationToken CancellationToken
        => _cancellation.Token;

    /// <summary>
    ///   Performs initialization of command execution.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The base implementation begins the cmdlet's ability to execute
    ///     asynchronous code via the <see cref="Run"/> method.
    ///   </para>
    /// </remarks>
    protected override void BeginProcessing()
    {
        BeginAsyncScope();
    }

    /// <summary>
    ///   Performs cleanup after command execution.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This method <b>must</b> be invoked on the main thread (the thread
    ///     that invoked <see cref="BeginProcessing"/> instance).
    ///   </para>
    ///   <para>
    ///     This method invokes pending actions dispatched to the main thread,
    ///     continuously, until all asynchronous actions queued by
    ///     <see cref="Run"/> complete.  This method then ends the cmdlet's
    ///     ability to execute asynchronous code.
    ///   </para>
    /// </remarks>
    protected override void EndProcessing()
    {
        EndAsyncScope();
    }

    /// <summary>
    ///   Requests cancellation of command execution.
    /// </summary>
    /// <remarks>
    ///   PowerShell invokes this method to interrupt a running command, such
    ///   as when the user presses CTRL-C.  PowerShell invokes this method on a
    ///   different thread than the main thread used for
    ///   <see cref="Cmdlet.BeginProcessing"/>,
    ///   <see cref="Cmdlet.ProcessRecord"/>, and
    ///   <see cref="Cmdlet.EndProcessing"/>.
    /// </remarks>
    protected override void StopProcessing()
    {
        WriteHost("Canceling...");
        _cancellation.Cancel();
    }

    /// <inheritdoc cref="AsyncCmdletScope.Run(Func{Task})"/>
    /// <exception cref="InvalidOperationException">
    ///   <see cref="BeginProcessing"/> has not been invoked.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="AsyncPSCmdlet"/> has been disposed.
    /// </exception>
    protected void Run(Func<Task> action)
    {
        RequireAsyncScope().Run(action);
    }

    /// <summary>
    ///   Invokes any pending actions dispatched to the main thread.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This method <b>must</b> be invoked on the main thread (the thread
    ///     that invoked <see cref="BeginProcessing"/> instance).
    ///   </para>
    ///   <para>
    ///     A long-running cmdlet might accrue pending actions dispatched to
    ///     the main thread long before the cmdlet enters a dispatch loop in
    ///     <see cref="WaitForAsyncActions"/> or <see cref="EndProcessing"/>.
    ///     To maximize responsiveness, use this method to invoke such pending
    ///     actions when an opportunity arises.
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   This method was invoked on a thread other than the thread that
    ///   invoked <see cref="BeginProcessing"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="AsyncPSCmdlet"/> has been disposed.
    /// </exception>
    protected void InvokePendingMainThreadActions()
    {
        RequireAsyncScope().InvokePendingMainThreadActions();
    }

    /// <summary>
    ///   Invokes pending actions dispatched to the main thread, continuously,
    ///   until all asynchronous actions queued by <see cref="Run"/> complete.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This method <b>must</b> be invoked on the main thread (the thread
    ///     that invoked <see cref="BeginProcessing"/> instance).
    ///   </para>
    ///   <para>
    ///     After invoking this method, the cmdlet remains able to execute
    ///     asynchronous code via the <see cref="Run"/> method.
    ///   </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   This method was invoked on a thread other than the thread that
    ///   invoked <see cref="BeginProcessing"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   The <see cref="AsyncPSCmdlet"/> has been disposed.
    /// </exception>
    protected void WaitForAsyncActions()
    {
        EndAsyncScope();
        BeginAsyncScope();
    }

    private void BeginAsyncScope()
    {
        _asyncScope = new(_cancellation.Token);
    }

    private void EndAsyncScope()
    {
        // Throws if invoked from non-main thread or if scope is disposed
        RequireAsyncScope().Complete();

        _asyncScope.Dispose();
        _asyncScope = null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        _asyncScope? .Dispose();
        _cancellation.Dispose();
    }

    [MemberNotNull(nameof(_asyncScope))]
    private AsyncCmdletScope RequireAsyncScope()
    {
        return _asyncScope ?? throw new InvalidOperationException(
            "This method requires prior invocation of BeginProcessing."
        );
    }

    #region PSCmdlet/ICmdlet (Re)Implementation

    /// <inheritdoc/>
    public new void WriteObject(object? obj)
    {
        Dispatcher.Post(() => base.WriteObject(obj));
    }

    /// <inheritdoc/>
    public new void WriteObject(object? obj, bool enumerate)
    {
        Dispatcher.Post(() => base.WriteObject(obj, enumerate));
    }

    /// <inheritdoc/>
    public new void WriteError(ErrorRecord record)
    {
        static void WriteError((PSCmdlet cmdlet, ErrorRecord record) x)
            => x.cmdlet.WriteError(x.record);

        Dispatcher.Invoke(WriteError, ((PSCmdlet) this, record));
    }

    /// <inheritdoc/>
    public new void WriteWarning(string? text)
    {
        static void WriteWarning((PSCmdlet cmdlet, string? text) x)
            => x.cmdlet.WriteWarning(x.text);

        Dispatcher.Invoke(WriteWarning, ((PSCmdlet) this, text));
    }

    /// <inheritdoc/>
    public new void WriteVerbose(string? text)
    {
        static void WriteVerbose((PSCmdlet cmdlet, string? text) x)
            => x.cmdlet.WriteVerbose(x.text);

        Dispatcher.Invoke(WriteVerbose, ((PSCmdlet) this, text));
    }

    /// <inheritdoc/>
    public new void WriteDebug(string? text)
    {
        static void WriteDebug((PSCmdlet cmdlet, string? text) x)
            => x.cmdlet.WriteDebug(x.text);

        Dispatcher.Invoke(WriteDebug, ((PSCmdlet) this, text));
    }

    /// <inheritdoc/>
    public void WriteHost(
        string?       text,
        bool          newLine         = true,
        ConsoleColor? foregroundColor = null,
        ConsoleColor? backgroundColor = null)
    {
        static void WriteHost((
            PSCmdlet      cmdlet,
            string?       text,
            bool          newLine,
            ConsoleColor? foregroundColor,
            ConsoleColor? backgroundColor) x)
            => x.cmdlet.WriteHost(x.text, x.newLine, x.foregroundColor, x.backgroundColor);

        Dispatcher.Invoke(
            WriteHost, ((PSCmdlet) this, text, newLine, foregroundColor, backgroundColor)
        );
    }

    /// <inheritdoc/>
    public new void WriteInformation(InformationRecord record)
    {
        static void WriteInformation((PSCmdlet cmdlet, InformationRecord record) x)
            => x.cmdlet.WriteInformation(x.record);

        Dispatcher.Invoke(WriteInformation, ((PSCmdlet) this, record));
    }

    /// <inheritdoc/>
    public new void WriteInformation(object? data, string?[]? tags)
    {
        static void WriteInformation((PSCmdlet cmdlet, object? data, string?[]? tags) x)
            => x.cmdlet.WriteInformation(x.data, x.tags);

        Dispatcher.Invoke(WriteInformation, ((PSCmdlet) this, data, tags));
    }

    /// <inheritdoc/>
    public new void WriteProgress(ProgressRecord record)
    {
        static void WriteProgress((PSCmdlet cmdlet, ProgressRecord record) x)
            => x.cmdlet.WriteProgress(x.record);

        Dispatcher.Invoke(WriteProgress, ((PSCmdlet) this, record));
    }

    /// <inheritdoc/>
    public new void WriteCommandDetail(string? text)
    {
        static void WriteCommandDetail((PSCmdlet cmdlet, string? text) x)
            => x.cmdlet.WriteCommandDetail(x.text);

        Dispatcher.Invoke(WriteCommandDetail, ((PSCmdlet) this, text));
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage(
        Justification = "Always throws in non-interactive test session; return statement unreachable."
    )]
    public new bool ShouldContinue(string? query, string? caption)
    {
        static bool ShouldContinue((PSCmdlet cmdlet, string? query, string? caption) x)
            => x.cmdlet.ShouldContinue(x.query, x.caption);

        return Dispatcher.Invoke(ShouldContinue, ((PSCmdlet) this, query, caption));
    }

    /// <inheritdoc/>
    public new bool ShouldContinue(
        string?  query,
        string?  caption,
        ref bool yesToAll,
        ref bool noToAll)
    {
        static (bool, bool, bool) ShouldContinue((
            PSCmdlet cmdlet,
            string?  query,
            string?  caption,
            bool     yesToAll,
            bool     noToAll) x)
        {
            return (
                x.cmdlet.ShouldContinue(
                    x.query, x.caption, ref x.yesToAll, ref x.noToAll
                ),
                x.yesToAll,
                x.noToAll
            );
        }

        (var result, yesToAll, noToAll) = Dispatcher.Invoke(
            ShouldContinue, ((PSCmdlet) this, query, caption, yesToAll, noToAll)
        );

        return result;
    }

    /// <inheritdoc/>
    public new bool ShouldContinue(
        string?  query,
        string?  caption,
        bool     hasSecurityImpact,
        ref bool yesToAll,
        ref bool noToAll)
    {
        static (bool, bool, bool) ShouldContinue((
            PSCmdlet cmdlet,
            string?  query,
            string?  caption,
            bool     hasSecurityImpact,
            bool     yesToAll,
            bool     noToAll) x)
        {
            return (
                x.cmdlet.ShouldContinue(
                    x.query, x.caption, x.hasSecurityImpact, ref x.yesToAll, ref x.noToAll
                ),
                x.yesToAll,
                x.noToAll
            );
        }

        (var result, yesToAll, noToAll) = Dispatcher.Invoke(
            ShouldContinue, ((PSCmdlet) this, query, caption, hasSecurityImpact, yesToAll, noToAll)
        );

        return result;
    }

    /// <inheritdoc/>
    public new bool ShouldProcess(string? target)
    {
        static bool ShouldProcess((PSCmdlet cmdlet, string? target) x)
            => x.cmdlet.ShouldProcess(x.target);

        return Dispatcher.Invoke(ShouldProcess, ((PSCmdlet) this, target));
    }

    /// <inheritdoc/>
    public new bool ShouldProcess(string? target, string? action)
    {
        static bool ShouldProcess((PSCmdlet cmdlet, string? target, string? action) x)
            => x.cmdlet.ShouldProcess(x.target, x.action);

        return Dispatcher.Invoke(ShouldProcess, ((PSCmdlet) this, target, action));
    }

    /// <inheritdoc/>
    public new bool ShouldProcess(string? verboseDescription, string? verboseWarning, string? caption)
    {
        static bool ShouldProcess((
            PSCmdlet cmdlet,
            string?  verboseDescription,
            string?  verboseWarning,
            string?  caption) x)
        {
            return x.cmdlet.ShouldProcess(
                x.verboseDescription, x.verboseWarning, x.caption
            );
        }

        return Dispatcher.Invoke(
            ShouldProcess, ((PSCmdlet) this, verboseDescription, verboseWarning, caption)
        );
    }

    /// <inheritdoc/>
    public new bool ShouldProcess(
        string?                 verboseDescription,
        string?                 verboseWarning,
        string?                 caption,
        out ShouldProcessReason shouldProcessReason)
    {
        static (bool, ShouldProcessReason) ShouldProcess((
            PSCmdlet cmdlet,
            string?  verboseDescription,
            string?  verboseWarning,
            string?  caption) x)
        {
            return (
                x.cmdlet.ShouldProcess(
                    x.verboseDescription, x.verboseWarning, x.caption, out var reason
                ),
                reason
            );
        }

        (var result, shouldProcessReason) = Dispatcher.Invoke(
            ShouldProcess, ((PSCmdlet) this, verboseDescription, verboseWarning, caption)
        );

        return result;
    }

    /// <inheritdoc/>
    public new void ThrowTerminatingError(ErrorRecord errorRecord)
    {
        Dispatcher.Post(() => base.ThrowTerminatingError(errorRecord));
    }

    #endregion
}
