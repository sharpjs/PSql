// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   An object that logs server messages received over a database connection.
/// </summary>
public interface ISqlMessageLogger
{
    /// <summary>
    ///   Logs the specified message.
    /// </summary>
    /// <param name="procedure">
    ///   The name of the stored procedure or remote procedure call that
    ///   generated the message.
    ///   Analogous to the T-SQL <c>ERROR_PROCEDURE</c> function.
    /// </param>
    /// <param name="line">
    ///   The line number within the stored procedure or command batch that
    ///   generated the message.
    ///   Analogous to the T-SQL <c>ERROR_LINE</c> function.
    /// </param>
    /// <param name="number">
    ///   The number that identifies the kind of message.
    ///   Analogous to the T-SQL <c>ERROR_NUMBER</c> function.
    /// </param>
    /// <param name="severity">
    ///   The severity level of the message.
    ///   Analogous to the T-SQL <c>ERROR_SEVERITY</c> function.
    /// </param>
    /// <param name="message">
    ///   The text of the message.
    ///   Analogous to the T-SQL <c>ERROR_MESSAGE</c> function.
    /// </param>
    void Log(string procedure, int line, int number, int severity, string? message);
}
