// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Internal;

/// <summary>
///   An object that can dispatch actions.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    ///   Dispatches the specified action.
    /// </summary>
    /// <param name="action">
    ///   The action to dispatch.
    /// </param>
    void Post(Action action);
}
