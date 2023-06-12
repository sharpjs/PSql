// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Represents a SQL command (statement batch) to execute against SQL Server,
///   Azure SQL Database, or compatible product.
/// </summary>
/// <remarks>
///   This type is a simplified proxy for <see cref="Mds.SqlCommand"/>.
/// </remarks>
public sealed class SqlCommand : IDisposable, IAsyncDisposable
{
    private readonly Mds.SqlCommand _command;

    /// <summary>
    ///   Creates a new <see cref="SqlCommand"/> that can execute commands on
    ///   the specified connection.
    /// </summary>
    /// <param name="connection">
    ///   The connection on which to execute commands.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connection"/> is <see langword="null"/>.
    /// </exception>
    internal SqlCommand(Mds.SqlConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        _command                    = connection.CreateCommand();
        _command.Connection         = connection;
        _command.RetryLogicProvider = connection.RetryLogicProvider;
        _command.CommandType        = CommandType.Text;
    }

    /// <summary>
    ///   Gets or sets the duration in seconds after which command execution
    ///   times out.  A value of <c>0</c> indicates no timeout: the command is
    ///   allowed to execute indefinitely.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///   Attempted to set a value less than <c>0</c>.
    /// </exception>
    public int CommandTimeout
    {
        get => _command.CommandTimeout;
        set => _command.CommandTimeout = value;
    }

    /// <summary>
    ///   Gets or sets the SQL command (statement batch) to execute.
    /// </summary>
    public string CommandText
    {
        get => _command.CommandText;
        set => _command.CommandText = value;
    }

    /// <summary>
    ///   Executes the command and projets its result sets to objects using the
    ///   specified delegates.
    /// </summary>
    /// <param name="createObject">
    ///   A delegate that creates a result object.
    ///   The method invokes this delegate once per result row.
    /// </param>
    /// <param name="setProperty">
    ///   A delegate that sets a property on a result object.
    ///   The method invokes this delegate once per column per result row.
    /// </param>
    /// <param name="useSqlTypes">
    ///   <see langword="false"/> to project fields using CLR types from the
    ///     <see cref="System"/> namespace, such as <see cref="int"/>.
    ///   <see langword="true"/> to project fields using SQL types from the
    ///     <see cref="System.Data.SqlTypes"/> namespace, such as
    ///     <see cref="System.Data.SqlTypes.SqlInt32"/>.
    /// </param>
    /// <returns>
    ///   A sequence of objects created by executing the command and projecting
    ///   each result row to an object using
    ///     <paramref name="createObject"/>,
    ///     <paramref name="setProperty"/>, and
    ///     <paramref name="useSqlTypes"/>,
    ///   in the order produced by the command.  If the command produces no
    ///   result rows, this method returns an empty sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="createObject"/> and/or
    ///   <paramref name="setProperty"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    ///   Thrown by <see cref="Mds.SqlConnection.Open"/>
    ///          or <see cref="Mds.SqlCommand.ExecuteReader"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   Thrown by <see cref="Mds.SqlConnection.Open"/>
    ///          or <see cref="Mds.SqlCommand.ExecuteReader"/>.
    /// </exception>
    /// <exception cref="IOException">
    ///   Thrown by <see cref="Mds.SqlConnection.Open"/>
    ///          or <see cref="Mds.SqlCommand.ExecuteReader"/>.
    /// </exception>
    /// <exception cref="SqlException">
    ///   Thrown by <see cref="Mds.SqlConnection.Open"/>
    ///          or <see cref="Mds.SqlCommand.ExecuteReader"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   Thrown by <see cref="Mds.SqlConnection.Open"/>
    ///          or <see cref="Mds.SqlCommand.ExecuteReader"/>.
    /// </exception>
    public IEnumerator<object> ExecuteAndProjectToObjects(
        Func   <object>                  createObject,
        Action <object, string, object?> setProperty,
        bool                             useSqlTypes = false)
    {
        if (createObject is null)
            throw new ArgumentNullException(nameof(createObject));
        if (setProperty is null)
            throw new ArgumentNullException(nameof(setProperty));

        if (_command.Connection.State == ConnectionState.Closed)
            _command.Connection.Open();

        var reader = _command.ExecuteReader();

        return new ObjectResultSet(reader, createObject, setProperty, useSqlTypes);
    }

    /// <summary>
    ///   Frees resources owned by the object.
    /// </summary>
    public void Dispose()
    {
        _command.Dispose();
    }

    /// <summary>
    ///   Frees resources owned by the object asynchronously.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return _command.DisposeAsync();
    }
}
