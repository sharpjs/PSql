// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Represents a connection to SQL Server, Azure SQL Database, or compatible
///   product.
/// </summary>
/// <remarks>
///   This type is a proxy for <c>Microsoft.Data.SqlClient.SqlConnection</c>.
/// </remarks>
public interface ISqlConnection : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///   Gets the underlying <c>Microsoft.Data.SqlClient.SqlConnection</c>.
    /// </summary>
    DbConnection UnderlyingConnection { get; }

    /// <summary>
    ///   Gets the connection string used to create the connection.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    ///   Gets a value indicating whether the connection is open.  The value is
    ///   <see langword="true"/> for new connections and transitions to
    ///   <see langword="false"/> permanently when the connection closes.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    ///   Gets a value indicating whether errors have been logged on the
    ///   connection since the most recent call to <see cref="ClearErrors"/>.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    ///   Sets <see cref="HasErrors"/> to <see langword="false"/>, forgetting
    ///   about any errors prevously logged on the connection.
    /// </summary>
    void ClearErrors();

    /// <summary>
    ///   Throws <see cref="DataException"/> if errors have been logged on the
    ///   connection since the most recent call to <see cref="ClearErrors"/>.
    /// </summary>
    /// <exception cref="DataException">
    ///   At least one error was logged on the connection since the most recent
    ///   call to <see cref="ClearErrors"/>.
    /// </exception>
    void ThrowIfHasErrors();

    /// <summary>
    ///   Creates a new <see cref="ISqlCommand"/> instance that can execute
    ///   commands on the connection.
    /// </summary>
    ISqlCommand CreateCommand();
}
