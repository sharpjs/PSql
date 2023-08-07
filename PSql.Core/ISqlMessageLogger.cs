// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   An object that logs server messages received over an
///   <see cref="ISqlConnection"/>.
/// </summary>
public interface ISqlMessageLogger
{
    /// <summary>
    ///   Logs the specified informational message.
    /// </summary>
    /// <param name="message">
    ///   The message to write.
    /// </param>
    void LogInformation(string message);

    /// <summary>
    ///   Logs the specified error message.
    /// </summary>
    /// <param name="message">
    ///   The message to write.
    /// </param>
    void LogError(string message);
}
