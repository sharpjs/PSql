// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

using System.Net;

using static SqlMessageConstants;

/// <summary>
///   A connection to a SQL Server database or Azure SQL Database instance.
/// </summary>
public class SqlConnection : IDisposable
{
    /// <summary>
    ///   Initializes a new <see cref="SqlConnection"/> instance.
    /// </summary>
    /// <param name="connectionString">
    ///   A string that specifies parameters for the connection.
    /// </param>
    /// <param name="credential">
    ///   The credential to use to authenticate with the server, or
    ///   <see langword="null"/> if the <paramref name="connectionString"/>
    ///   contains all information necessary to authenticate.
    /// </param>
    /// <param name="logger">
    ///   The logger for server messages received over the connection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connectionString"/> or
    ///   <paramref name="logger"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="connectionString"/> is invalid.
    /// </exception>
    /// <exception cref="DbException">
    ///   A connection-level error occurred while opening the connection.
    /// </exception>
    public SqlConnection(
        string             connectionString,
        NetworkCredential? credential,
        ISqlMessageLogger  logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);

        ConnectionString = connectionString;
        Logger           = logger;

        Connection = credential is null
            ? new(connectionString)
            : new(connectionString, ToSqlCredential(credential));

        Connection.RetryLogicProvider                = RetryLogicProvider;
        Connection.FireInfoMessageEventOnUserErrors  = true;
        Connection.InfoMessage                      += HandleMessage;
        Connection.Disposed                         += HandleUnexpectedDisposal;

        Command                    = Connection.CreateCommand();
        Command.CommandType        = CommandType.Text;
        Command.RetryLogicProvider = RetryLogicProvider;
    }

    /// <summary>
    ///   Gets the retry logic for connections.
    /// </summary>
    protected static SqlRetryLogicBaseProvider RetryLogicProvider { get; }
        = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(new()
        {
            NumberOfTries   = 5,
            DeltaTime       = TimeSpan.FromSeconds(2),
            MaxTimeInterval = TimeSpan.FromMinutes(2),
        });

    /// <summary>
    ///   Gets the string that specifies parameters for the connection.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    ///   Gets the underlying SqlClient connection.
    /// </summary>
    protected Mds.SqlConnection Connection { get; }

    /// <summary>
    ///   Gets the underlying SqlClient command.
    /// </summary>
    protected Mds.SqlCommand Command { get; }

    /// <summary>
    ///   Gets the logger for server messages received over the connection.
    /// </summary>
    public ISqlMessageLogger Logger { get; }

    /// <summary>
    ///   Gets whether one or more error messages have been received over the
    ///   connection since the most recent invocation of
    ///   <see cref="ClearErrors"/>.
    /// </summary>
    protected bool HasErrors { get; private set; }

    /// <summary>
    ///   Gets whether the connection is open.
    /// </summary>
    public bool IsOpen => Connection.State is ConnectionState.Open;

    /// <summary>
    ///   Throws an exception if one or more error messages have been received
    ///   over the connection since the most recent invocation of
    ///   <see cref="ClearErrors"/>.
    /// </summary>
    /// <exception cref="DataException">
    ///   One or more error messages have been received over the connection
    ///   since the most recent invocation of <see cref="ClearErrors"/>.
    /// </exception>
    protected internal void ThrowIfHasErrors()
    {
        if (HasErrors)
            throw new DataException("An error occurred while executing the SQL batch.");
    }

    /// <summary>
    ///   Resets error-handling state, forgetting any error messages that have
    ///   been received over the connection.
    /// </summary>
    protected void ClearErrors()
    {
        HasErrors = false;
    }

    /// <summary>
    ///   Configures the <see cref="Command"/> object.
    /// </summary>
    /// <param name="sql">
    ///   The command text to execute.
    /// </param>
    /// <param name="timeout">
    ///   The command timeout, in seconds, or <c>0</c> for no timeout.
    /// </param>
    /// <returns>
    ///   The value of the <see cref="Command"/> property, configured with the
    ///   specified <paramref name="sql"/> and <paramref name="timeout"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="timeout"/> is less than <c>0</c>.
    /// </exception>
    protected SqlCommand SetUpCommand(
        string                     sql,
        int                        timeout = 0)
    {
        ArgumentNullException.ThrowIfNull(sql);

        if (timeout < 0)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        ClearErrors();

        Command.CommandText    = sql;
        Command.CommandTimeout = timeout;

        return Command;
    }

    /// <summary>
    ///   Ensures that the connection is open.
    /// </summary>
    protected void AutoOpen()
    {
        if (Connection.State == ConnectionState.Closed)
            Connection.Open();
    }

    /// <summary>
    ///   Executes the specified SQL batch and projects its result rows to
    ///   objects using the specified builder.
    /// </summary>
    /// <param name="sql">
    ///   The SQL batch to execute.
    /// </param>
    /// <param name="builder">
    ///   The strategy to project result rows to objects.
    /// </param>
    /// <param name="timeout">
    ///   The command timeout, in seconds, or <c>0</c> for no timeout.
    /// </param>
    /// <param name="useSqlTypes">
    ///   <see langword="false"/> to project column values using CLR types from
    ///     the <see cref="System"/> namespace, such as <see cref="int"/>.
    ///   <see langword="true"/> to project column values using SQL types from
    ///     the <see cref="System.Data.SqlTypes"/> namespace, such as
    ///     <see cref="System.Data.SqlTypes.SqlInt32"/>.
    /// </param>
    /// <returns>
    ///   A sequence of objects created by executing the <paramref name="sql"/>
    ///   batch and projecting each result row to an object using the
    ///   <paramref name="builder"/>.  If the command produces no result rows,
    ///   this method returns an empty sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="sql"/> or
    ///   <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="timeout"/> is less than <c>0</c>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    ///   Thrown by the underlying ADO.NET connection or command objects.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   Thrown by the underlying ADO.NET connection or command objects.
    /// </exception>
    /// <exception cref="IOException">
    ///   Thrown by the underlying ADO.NET connection or command objects.
    /// </exception>
    /// <exception cref="DbException">
    ///   Thrown by the underlying ADO.NET connection or command objects.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    ///   Thrown by the underlying ADO.NET connection or command objects.
    /// </exception>
    public IEnumerator<T> ExecuteAndProjectTo<T>(
        string            sql,
        IObjectBuilder<T> builder,
        int               timeout     = 0,
        bool              useSqlTypes = false)
    {
        SetUpCommand(sql, timeout);
        AutoOpen();

        var reader = Command.ExecuteReader();

        return new ObjectResultSet<T>(this, reader, builder, useSqlTypes);
    }

    /// <summary>
    ///   Closes the connection and frees resources owned by it.
    /// </summary>
    public void Dispose()
    {
        // Disposal is now expected
        Connection.Disposed -= HandleUnexpectedDisposal;

        Command   .Dispose();
        Connection.Dispose();

        GC.SuppressFinalize(this);
    }

    private static SqlCredential ToSqlCredential(NetworkCredential credential)
    {
        var username = credential.UserName;
        var password = credential.SecurePassword;

        if (!password.IsReadOnly())
            (password = password.Copy()).MakeReadOnly();

        return new(username, password);
    }

    private void HandleMessage(object sender, SqlInfoMessageEventArgs e)
    {
        const string NonProcedureLocationName = "(batch)";

        foreach (SqlError error in e.Errors)
        {
            Assume.NotNull(error); // SqlClient code appears to assume this

            // Mark current command if failed
            HasErrors |= error.Class > MaxInformationalSeverity;

            Logger.Log(
                error.Procedure.NullIfEmpty() ?? NonProcedureLocationName,
                error.LineNumber,
                error.Number,
                error.Class,
                error.Message
            );
        }
    }

    private static void HandleUnexpectedDisposal(object? sender, EventArgs e)
    {
        throw new DataException(
            "The connection to the database server was closed unexpectedly."
        );
    }
}
