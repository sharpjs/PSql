// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Represents a SQL command (statement batch) to execute against
///   SQL Server, Azure SQL Database, or compatible product.
/// </summary>
/// <remarks>
///   This type is a proxy for <see cref="Mds.SqlCommand"/>.
/// </remarks>
public class SqlCommand : IDisposable
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
    public /*TODO:internal*/ SqlCommand(Mds.SqlConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        _command             = connection.CreateCommand();
        _command.Connection  = connection;
        _command.CommandType = CommandType.Text;
    }

    /// <summary>
    ///   Gets or sets the duration in seconds after which command execution
    ///   times out.  A value of <c>0</c> indicates no timeout: a command is
    ///   allowed to execute indefinitely.
    /// </summary>
    /// <exception cref="AggregateException">
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
    ///   Executes the command and projets its result sets to PowerShell
    ///   object (<see cref="PSObject"/>) instances.
    /// </summary>
    /// <param name="useSqlTypes">
    ///   <see langword="false"/> to project fields using CLR types from the
    ///     <see cref="System"/> namespace, such as <see cref="int"/>.
    ///   <see langword="true"/> to project fields using SQL types from the
    ///     <see cref="System.Data.SqlTypes"/> namespace, such as
    ///     <see cref="System.Data.SqlTypes.SqlInt32"/>.
    /// </param>
    public IEnumerator<object> ExecuteAndProjectToObjects(
        Func<object> objectCreator,
        Action<object, string, object?> propertySetter,
        bool useSqlTypes)
    {
        return PSqlClient.Instance.ExecuteAndProject(
            _command, objectCreator, propertySetter, useSqlTypes
        );
    }

    /// <summary>
    ///   Frees resources owned by the object.
    /// </summary>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Frees resources owned by the object.
    /// </summary>
    /// <param name="managed">
    ///   Whether to dispose managed resources.  Unmanaged are always
    ///   disposed.
    /// </param>
    protected virtual void Dispose(bool managed)
    {
        if (managed)
            _command.Dispose();
    }
}
