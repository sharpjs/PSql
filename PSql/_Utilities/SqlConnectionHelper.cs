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
        ///   context and database name.
        /// </summary>
        /// <param name="connection">
        ///   The shared connection.  If provided, the method returns this
        ///   connection.
        /// </param>
        /// <param name="context">
        ///   An object containing information necessary to connect to a SQL
        ///   Server or compatible database if <paramref name="connection"/> is
        ///   <c>null</c>.  If not provided, the method will use a context with
        ///   default property values as necessary.
        /// </param>
        /// <param name="databaseName">
        ///   The name of the database to which to connect if
        ///   <paramref name="connection"/> is <c>null</c>.  If not provided,
        ///   the method connects to the default database for the context.
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
