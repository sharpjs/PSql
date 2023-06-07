// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Represents a connection to SQL Server, Azure SQL Database, or
///   compatible product.
/// </summary>
/// <remarks>
///   This type is a proxy for <c>Microsoft.Data.SqlClient.SqlConnection.</c>
/// </remarks>
public class SqlConnection : IDisposable
{
    private readonly Mds.SqlConnection _connection;

    /// <summary>
    ///   Creates a new <see cref="SqlConnection"/> instance for the specified
    ///   context and database name, logging server messages via the specified
    ///   cmdlet.
    /// </summary>
    /// <param name="context">
    ///   An object containing information necessary to connect to a database.
    ///   If not provided, the constructor will use a context with default
    ///   property values.
    /// </param>
    /// <param name="databaseName">
    ///   The name of the database to which to connect.  If not provided, the
    ///   constructor connects to the default database for the context.
    /// </param>
    /// <param name="cmdlet">
    ///   The cmdlet whose
    ///     <see cref="Cmdlet.WriteHost(string, bool, ConsoleColor?, ConsoleColor?)"/>
    ///   and
    ///     <see cref="System.Management.Automation.Cmdlet.WriteWarning(string)"/>
    ///   methods will be used to print messges received from the server.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="cmdlet"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.Data.Common.DbException">
    ///   A connection-level error occurred while opening the connection.
    /// </exception>
    internal SqlConnection(SqlContext? context, string? databaseName, Cmdlet cmdlet)
    {
        const SqlClientVersion Version = SqlClientVersion.Latest;

        if (cmdlet is null)
            throw new ArgumentNullException(nameof(cmdlet));

        context ??= new SqlContext();

        var client           = PSqlClient.Instance;
        var connectionString = context.GetConnectionString(databaseName, Version, true);
        var credential       = context.Credential;
        var writeInformation = new Action<string>(s => cmdlet.WriteHost   (s));
        var writeWarning     = new Action<string>(s => cmdlet.WriteWarning(s));

        var passCredentialSeparately
            =  !credential.IsNullOrEmpty()
            && !context.ExposeCredentialInConnectionString;

        _connection = passCredentialSeparately
            ? client.Connect(
                connectionString,
                credential!.UserName,
                credential!.Password,
                writeInformation,
                writeWarning
            )
            : client.Connect(
                connectionString,
                writeInformation,
                writeWarning
            );
    }

    /// <summary>
    ///   Gets the connection string used to create this connection.  The
    ///   connection string includes server name, database name, and other
    ///   parameters that control the initial opening of the connection.
    /// </summary>
    public string ConnectionString
        => _connection.ConnectionString;

    /// <summary>
    ///   Gets a value indicating whether the connection is open.  The
    ///   value is <see langword="true"/> (open) for new connections and
    ///   transitions permanently to <see langword="false"/> (closed) when the
    ///   connection closes.
    /// </summary>
    public bool IsOpen
        => (int) _connection.State == (int) ConnectionState.Open;

    /// <summary>
    ///   Gets a value indicating whether errors have been logged on the
    ///   connection.
    /// </summary>
    public bool HasErrors
        => PSqlClient.Instance.HasErrors(_connection);

    /// <summary>
    ///   Sets <see cref="HasErrors"/> to <see langword="false"/>, forgetting
    ///   about any errors prevously logged on the connection.
    /// </summary>
    internal void ClearErrors()
    {
        PSqlClient.Instance.ClearErrors(_connection);
    }

    /// <summary>
    ///   Creates a new <see cref="SqlCommand"/> instance that can execute
    ///   commands on the connection.
    /// </summary>
    internal SqlCommand CreateCommand()
    {
        return new SqlCommand(_connection);
    }

    /// <summary>
    ///   Closes the connection and frees resources owned by it.
    /// </summary>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Closes the connection and frees resources owned by it.
    /// </summary>
    /// <param name="managed">
    ///   Whether to dispose managed resources.  This method always disposes
    ///   unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool managed)
    {
        if (managed)
        {
            // Indicate that disconnection is expected
            PSqlClient.Instance.SetDisconnecting(_connection);

            // Disconnect
            _connection.Dispose();
        }
    }
}
