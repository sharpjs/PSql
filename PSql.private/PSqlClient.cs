// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Security;

namespace PSql;

/// <summary>
///   Top-level interface between PSql and PSql.private
/// </summary>
public class PSqlClient
{
    public static PSqlClient Instance { get; } = new();

    static PSqlClient()
    {
        SniLoader.Load();
    }

    /// <summary>
    ///   Creates and opens a new <see cref="Mds.SqlConnection"/> instance
    ///   using the specified connection string, logging server messages
    ///   via the specified delegates.
    /// </summary>
    /// <param name="connectionString">
    ///   Gets the connection string used to create this connection.  The
    ///   connection string includes server name, database name, and other
    ///   parameters that control the initial opening of the connection.
    /// </param>
    /// <param name="writeInformation">
    ///   Delegate that logs server informational messages.
    /// </param>
    /// <param name="writeWarning">
    ///   Delegate that logs server warning or error messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connectionString"/>,
    ///   <paramref name="writeInformation"/>, and/or
    ///   <paramref name="writeWarning"/> is <see langword="null"/>.
    /// </exception>
    public Mds.SqlConnection Connect(
        string         connectionString,
        Action<string> writeInformation,
        Action<string> writeWarning)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (writeInformation is null)
            throw new ArgumentNullException(nameof(writeInformation));
        if (writeWarning is null)
            throw new ArgumentNullException(nameof(writeWarning));

        return ConnectCore(
            new Mds.SqlConnection(connectionString),
            writeInformation,
            writeWarning
        );
    }

    /// <summary>
    ///   Creates and opens a new <see cref="Mds.SqlConnection"/> instance
    ///   using the specified connection string and credential, logging
    ///   server messages via the specified delegates.
    /// </summary>
    /// <param name="connectionString">
    ///   Gets the connection string used to create this connection.  The
    ///   connection string includes server name, database name, and other
    ///   parameters that control the initial opening of the connection.
    /// </param>
    /// <param name="username">
    ///   The username to present for authentication.
    /// </param>
    /// <param name="password">
    ///   The password to present for authentication.
    /// </param>
    /// <param name="writeInformation">
    ///   Delegate that logs server informational messages.
    /// </param>
    /// <param name="writeWarning">
    ///   Delegate that logs server warning or error messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connectionString"/>,
    ///   <paramref name="username"/>,
    ///   <paramref name="password"/>,
    ///   <paramref name="writeInformation"/>, and/or
    ///   <paramref name="writeWarning"/> is <see langword="null"/>.
    /// </exception>
    public Mds.SqlConnection Connect(
        string         connectionString,
        string         username,
        SecureString   password,
        Action<string> writeInformation,
        Action<string> writeWarning)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (username is null)
            throw new ArgumentNullException(nameof(username));
        if (password is null)
            throw new ArgumentNullException(nameof(password));
        if (writeInformation is null)
            throw new ArgumentNullException(nameof(writeInformation));
        if (writeWarning is null)
            throw new ArgumentNullException(nameof(writeWarning));

        if (!password.IsReadOnly())
            (password = password.Copy()).MakeReadOnly();

        var credential = new SqlCredential(username, password);

        return ConnectCore(
            new Mds.SqlConnection(connectionString, credential),
            writeInformation,
            writeWarning
        );
    }

    private Mds.SqlConnection ConnectCore(
        Mds.SqlConnection  connection,
        Action<string> writeInformation,
        Action<string> writeWarning)
    {
        var info = null as ConnectionInfo;

        try
        {
            info = ConnectionInfo.Get(connection);

            SqlConnectionLogger.Use(connection, writeInformation, writeWarning);

            connection.Open();
            return connection;
        }
        catch
        {
            if (info != null)
                info.IsDisconnecting = true;

            connection?.Dispose();
            throw;
        }
    }

    /// <summary>
    ///   Returns a value indicating whether errors have been logged on the
    ///   specified connection.
    /// </summary>
    /// <param name="connection">
    ///   The connection to check.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connection"/> is <see langword="null"/>.
    /// </exception>
    public bool HasErrors(Mds.SqlConnection connection)
    {
        return ConnectionInfo.Get(connection).HasErrors;
    }

    /// <summary>
    ///   Clears any errors prevously logged on the specified connection.
    /// </summary>
    /// <param name="connection">
    ///   The connection for which to clear error state.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connection"/> is <see langword="null"/>.
    /// </exception>
    public void ClearErrors(Mds.SqlConnection connection)
    {
        ConnectionInfo.Get(connection).HasErrors = false;
    }

    /// <summary>
    ///   Indicates that the specified connection is expected to
    ///   disconnect.
    /// </summary>
    /// <param name="connection">
    ///   The connection that is expected to disconnect.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connection"/> is <see langword="null"/>.
    /// </exception>
    public void SetDisconnecting(Mds.SqlConnection connection)
    {
        ConnectionInfo.Get(connection).IsDisconnecting = true;
    }

    /// <summary>
    ///   Executes the specified <see cref="SqlCommand"/> and projects
    ///   results to objects using the specified delegates.
    /// </summary>
    /// <param name="command">
    ///   The command to execute.
    /// </param>
    /// <param name="createObject">
    ///   Delegate that creates a result object.
    /// </param>
    /// <param name="setProperty">
    ///   Delegate that sets a property on a result object.
    /// </param>
    /// <param name="useSqlTypes">
    ///   <see langword="false"/> to project fields using CLR types from the
    ///     <see cref="System"/> namespace, such as <see cref="int"/>.
    ///   <see langword="true"/> to project fields using SQL types from the
    ///     <see cref="System.Data.SqlTypes"/> namespace, such as
    ///     <see cref="System.Data.SqlTypes.SqlInt32"/>.
    /// </param>
    /// <returns>
    ///   A sequence of objects created by executing
    ///     <paramref name="command"/>
    ///   and projecting each result row to an object using
    ///     <paramref name="createObject"/>,
    ///     <paramref name="setProperty"/>, and
    ///     <paramref name="useSqlTypes"/>,
    ///   in the order produced by the command.  If the command produces no
    ///   result rows, this method returns an empty sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="command"/>,
    ///   <paramref name="createObject"/>, and/or
    ///   <paramref name="setProperty"/> is <see langword="null"/>.
    /// </exception>
    internal IEnumerator<object> ExecuteAndProject(
        Mds.SqlCommand                  command,
        Func<object>                    createObject,
        Action<object, string, object?> setProperty,
        bool                            useSqlTypes = false)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));
        if (createObject is null)
            throw new ArgumentNullException(nameof(createObject));
        if (setProperty is null)
            throw new ArgumentNullException(nameof(setProperty));

        if (command.Connection.State == ConnectionState.Closed)
            command.Connection.Open();

        var reader = command.ExecuteReader();

        return new ObjectResultSet(reader, createObject, setProperty, useSqlTypes);
    }
}
