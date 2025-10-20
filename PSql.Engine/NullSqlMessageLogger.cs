// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   An <see cref="ISqlMessageLogger"/> that does nothing.
/// </summary>
public class NullSqlMessageLogger : ISqlMessageLogger
{
    private NullSqlMessageLogger() { }

    /// <summary>
    ///   Gets the singleton <see cref="NullSqlMessageLogger"/> instance.
    /// </summary>
    public static NullSqlMessageLogger Instance { get; } = new();

    /// <inheritdoc/>
    public void Log(string procedure, int line, int number, int severity, string? message)
    {
        // NOP
    }
}
