// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <inheritdoc/>
internal sealed class SqlCommand : ISqlCommand
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

    /// <inheritdoc/>
    public DbCommand UnderlyingCommand
        => _command;

    /// <inheritdoc/>
    public int CommandTimeout
    {
        get => _command.CommandTimeout;
        set => _command.CommandTimeout = value;
    }

    /// <inheritdoc/>
    public string CommandText
    {
        get => _command.CommandText;
        set => _command.CommandText = value;
    }

    /// <inheritdoc/>
    public IEnumerator<PSObject> ExecuteAndProjectToPSObjects(bool useSqlTypes = false)
    {
        if (_command.Connection.State == ConnectionState.Closed)
            _command.Connection.Open();

        var reader = _command.ExecuteReader();

        return new ObjectResultSet(reader, useSqlTypes);
    }

    /// <inheritdoc/>
    public void Dispose()
        => _command.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
        => _command.DisposeAsync();
}
