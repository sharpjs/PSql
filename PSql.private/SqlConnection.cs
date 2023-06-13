// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Security;

namespace PSql;

/// <summary>
///   Represents a connection to SQL Server, Azure SQL Database, or compatible
///   product.
/// </summary>
/// <remarks>
///   This type is a simplified proxy for <see cref="Mds.SqlConnection"/>.
/// </remarks>
public sealed class SqlConnection : IDisposable, IAsyncDisposable
{
    private static readonly SqlRetryLogicBaseProvider RetryLogic
        = SqlConfigurableRetryFactory.CreateExponentialRetryProvider(new()
        {
            NumberOfTries   = 5,
            DeltaTime       = TimeSpan.FromSeconds(2),
            MaxTimeInterval = TimeSpan.FromMinutes(2),
        });

    private readonly Mds.SqlConnection _connection;
    private readonly ICmdlet           _cmdlet;

    /// <summary>
    ///   Initializes and opens a new <see cref="SqlConnection"/> instance with
    ///   the specified connection string and logging delegates.
    /// </summary>
    /// <param name="connectionString">
    ///   A string that specifies parameters for the connection.
    /// </param>
    /// <param name="cmdlet">
    ///   The cmdlet whose output methodes to use to log server messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connectionString"/> and/or
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="connectionString"/> is invalid.
    /// </exception>
    /// <exception cref="DbException">
    ///   A connection-level error occurred while opening the connection.
    /// </exception>
    public SqlConnection(string connectionString, ICmdlet cmdlet)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        _connection = new Mds.SqlConnection(connectionString);
        _cmdlet     = cmdlet;

        Initialize();
    }

    /// <summary>
    ///   Initializes and opens a new <see cref="SqlConnection"/> instance with
    ///   the specified connection string, credential, and logging delegates.
    /// </summary>
    /// <param name="connectionString">
    ///   A string that specifies parameters for the connection.
    /// </param>
    /// <param name="username">
    ///   The username to use to authenticate with the database server.
    /// </param>
    /// <param name="password">
    ///   The password to use to authenticate with the database server.
    /// </param>
    /// <param name="cmdlet">
    ///   The cmdlet whose output methodes to use to log server messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="connectionString"/>,
    ///   <paramref name="username"/>,
    ///   <paramref name="password"/>, and/or
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="connectionString"/> is invalid.
    /// </exception>
    /// <exception cref="DbException">
    ///   A connection-level error occurred while opening the connection.
    /// </exception>
    public SqlConnection(
        string       connectionString,
        string       username,
        SecureString password,
        ICmdlet      cmdlet)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));
        if (username is null)
            throw new ArgumentNullException(nameof(username));
        if (password is null)
            throw new ArgumentNullException(nameof(password));
        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        if (!password.IsReadOnly())
            (password = password.Copy()).MakeReadOnly();

        var credential = new SqlCredential(username, password);

        _connection = new Mds.SqlConnection(connectionString, credential);
        _cmdlet     = cmdlet;

        Initialize();
    }

    private void Initialize()
    {
        _connection.RetryLogicProvider                = RetryLogic;
        _connection.FireInfoMessageEventOnUserErrors  = true;
        _connection.InfoMessage                      += HandleMessage;
        _connection.Disposed                         += HandleUnexpectedClose;

        try
        {
            _connection.Open();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <summary>
    ///   Gets the connection string used to create the connection.
    /// </summary>
    public string ConnectionString
        => _connection.ConnectionString;

    /// <summary>
    ///   Gets a value indicating whether the connection is open.  The value is
    ///   <see langword="true"/> for new connections and transitions to
    ///   <see langword="false"/> permanently when the connection closes.
    /// </summary>
    public bool IsOpen
        => (int) _connection.State == (int) ConnectionState.Open;

    /// <summary>
    ///   Gets a value indicating whether errors have been logged on the
    ///   connection since the most recent call to <see cref="ClearErrors"/>.
    /// </summary>
    public bool HasErrors { get; private set; }

    /// <summary>
    ///   Sets <see cref="HasErrors"/> to <see langword="false"/>, forgetting
    ///   about any errors prevously logged on the connection.
    /// </summary>
    public void ClearErrors()
    {
        HasErrors = false;
    }

    /// <summary>
    ///   Throws <see cref="DataException"/> if errors have been logged on the
    ///   connection since the most recent call to <see cref="ClearErrors"/>.
    /// </summary>
    /// <exception cref="DataException">
    ///   At least one error was logged on the connection since the most recent
    ///   call to <see cref="ClearErrors"/>.
    /// </exception>
    public void ThrowIfHasErrors()
    {
        if (HasErrors)
            throw new DataException("An error occurred while executing the SQL batch.");
    }

    /// <summary>
    ///   Creates a new <see cref="SqlCommand"/> instance that can execute
    ///   commands on the connection.
    /// </summary>
    public SqlCommand CreateCommand()
    {
        return new SqlCommand(_connection);
    }

    /// <summary>
    ///   Closes the connection and frees resources owned by it.
    /// </summary>
    public void Dispose()
    {
        // Closing is now expected
        _connection.Disposed -= HandleUnexpectedClose;

        // Close the connection
        _connection.Dispose();
    }

    /// <summary>
    ///   Closes the connection and frees resources owned by it asynchronously.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        // Closing is now expected
        _connection.Disposed -= HandleUnexpectedClose;

        // Close the connection
        return _connection.DisposeAsync();
    }

    private void HandleMessage(object sender, SqlInfoMessageEventArgs e)
    {
        const int MaxInformationalSeverity = 10;

        foreach (SqlError? error in e.Errors)
        {
            if (error is null)
                continue;

            if (error.Class <= MaxInformationalSeverity)
                LogInformation(error);
            else
                LogWarning(error);
        }
    }

    private void LogInformation(SqlError error)
    {
        _cmdlet.WriteHost(error.Message);
    }

    private void LogWarning(SqlError error)
    {
        _cmdlet.WriteWarning(Format(error));

        // Mark current command as failed
        HasErrors = true;
    }

    private static string Format(SqlError error)
    {
        const string NonProcedureLocationName = "(batch)";

        var procedure
            =  error.Procedure.NullIfEmpty()
            ?? NonProcedureLocationName;

        return $"{procedure}:{error.LineNumber}: E{error.Class}: {error.Message}";
    }

    private static void HandleUnexpectedClose(object? sender, EventArgs e)
    {
        throw new DataException(
            "The connection to the database server was closed unexpectedly."
        );
    }
}
