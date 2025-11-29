// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

extern alias Engine;

using System.Net;
using PSql.Commands;

namespace PSql;

/// <summary>
///   An open connection to SQL Server, Azure SQL Database, or compatible
///   product.
/// </summary>
public sealed class SqlConnection : IDisposable
{
    private readonly E.SqlConnection _connection;

    internal SqlConnection(string connectionString, NetworkCredential? credential, ICmdlet cmdlet)
    {
        _connection = new(connectionString, credential, new CmdletSqlMessageLogger(cmdlet));
    }

    /// <summary>
    ///   Gets the inner connection wrapped by this object.
    /// </summary>
    internal E.SqlConnection InnerConnection => _connection;

    /// <summary>
    ///   Closes the connection and frees resources owned by it.
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
    }
}
