// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Internal;

/// <summary>
///   A dispatcher that executes dispatched actions immediately.
/// </summary>
internal sealed class ImmediateDispatcher : IDispatcher
{
    /// <summary>
    ///   Gets the singleton <see cref="ImmediateDispatcher"/> instance.
    /// </summary>
    public static IDispatcher Instance { get; } = new ImmediateDispatcher();

    private ImmediateDispatcher() { }

    /// <inheritdoc/>
    public void Post(Action action)
        => action();
}
