// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Helper methods associated with <see cref="SqlConnection"/>.
/// </summary>
internal static class SqlConnectionHelper
{
    /// <summary>
    ///   Returns the specified shared <see cref="SqlConnection"/> instance if
    ///   provided, or creates a new, owned instance using the specified
    ///   context, database name, and cmdlet.
    /// </summary>
    /// <param name="connection">
    ///   The shared connection.  If provided, the method returns this
    ///   connection.
    /// </param>
    /// <param name="context">
    ///   An object containing information necessary to connect to a database
    ///   if <paramref name="connection"/> is <see langword="null"/>.  If not
    ///   provided, the method will use a context with default property values
    ///   as necessary.
    /// </param>
    /// <param name="databaseName">
    ///   The name of the database to which to connect if <paramref name="connection"/>
    ///   is <see langword="null"/>.  If not provided, the method connects to
    ///   the default database for the context.
    /// </param>
    /// <param name="cmdlet">
    ///   A cmdlet whose
    ///     <see cref="Cmdlet.WriteHost(string, bool, ConsoleColor?, ConsoleColor?)"/>
    ///   and
    ///     <see cref="System.Management.Automation.Cmdlet.WriteWarning(string)"/>
    ///   methods will be used to print messges received from the server over
    ///   the new connection created if <paramref name="connection"/> is
    ///   <see langword="null"/>.
    /// </param>
    /// <returns>
    ///   A tuple consisting of the resulting connection and a value that
    ///   indicates whether the caller owns the connection and must ensure its
    ///   disposal.  If <paramref name="connection"/> is provided, the method
    ///   returns that connection and <see langword="false"/> (shared).
    ///   Otherwise, the method creates a new connection as specified by
    ///   <paramref name="context"/> and <paramref name="databaseName"/> and
    ///   returns the connection and <see langword="true"/> (owned).
    /// </returns>
    internal static (SqlConnection, bool owned) EnsureConnection(
        SqlConnection? connection,
        SqlContext?    context,
        string?        databaseName,
        Cmdlet         cmdlet)
    {
        return connection != null
            ? (connection,                                       owned: false)
            : ((context ?? new()).CreateConnection(databaseName, cmdlet), owned: true );
    }
}
