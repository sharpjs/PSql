/*
    Copyright 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

namespace PSql
{
    /// <summary>
    ///   Helper methods associated with <see cref="SqlConnection"/>.
    /// </summary>
    internal static class SqlConnectionHelper
    {
        /// <summary>
        ///   Returns the specified shared <see cref="SqlConnection"/> instance
        ///   if provided, or creates a new, owned instance using the specified
        ///   context, database name, and cmdlet.
        /// </summary>
        /// <param name="connection">
        ///   The shared connection.  If provided, the method returns this
        ///   connection.
        /// </param>
        /// <param name="context">
        ///   An object containing information necessary to connect to a
        ///   database if <paramref name="connection"/> is <c>null</c>.  If not
        ///   provided, the method will use a context with default property
        ///   values as necessary.
        /// </param>
        /// <param name="databaseName">
        ///   The name of the database to which to connect if
        ///   <paramref name="connection"/> is <c>null</c>.  If not provided,
        ///   the method connects to the default database for the context.
        /// </param>
        /// <param name="cmdlet">
        ///   A cmdlet whose
        ///     <see cref="Cmdlet.WriteHost(string, bool, ConsoleColor?, ConsoleColor?)"/>
        ///   and
        ///     <see cref="System.Management.Automation.Cmdlet.WriteWarning(string)"/>
        ///   methods will be used to print messges received from the server
        ///   over the new connection created if <paramref name="connection"/>
        ///   is <c>null</c>.
        /// </param>
        /// <returns>
        ///   A tuple consisting of the resulting connection and a value that
        ///   indicates whether the caller owns the connection and must ensure
        ///   its disposal.  If <paramref name="connection"/> is provided, the
        ///   method returns that connection and <c>false</c> (shared).
        ///   Otherwise, the method creates a new connection as specified by
        ///   <paramref name="context"/> and <paramref name="databaseName"/>
        ///   and returns the connection and <c>true</c> (owned).
        /// </returns>
        internal static (SqlConnection, bool owned) EnsureConnection(
            SqlConnection? connection,
            SqlContext?    context,
            string?        databaseName,
            Cmdlet         cmdlet)
        {
            return connection != null
                ? (connection,                                       owned: false)
                : (new SqlConnection(context, databaseName, cmdlet), owned: true );
        }
    }
}
