// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Represents a SQL command (statement batch) to execute against SQL Server,
///   Azure SQL Database, or compatible product.
/// </summary>
/// <remarks>
///   This type is a proxy for <c>Microsoft.Data.SqlClient.SqlCommand</c>.
/// </remarks>
public interface ISqlCommand : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///   Gets the underlying <c>Microsoft.Data.SqlClient.SqlCommand</c>.
    /// </summary>
    DbCommand UnderlyingCommand { get; }

    /// <summary>
    ///   Gets or sets the duration in seconds after which command execution
    ///   times out.  A value of <c>0</c> indicates no timeout: the command is
    ///   allowed to execute indefinitely.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///   Attempted to set a value less than <c>0</c>.
    /// </exception>
    int CommandTimeout { get; set; }

    /// <summary>
    ///   Gets or sets the SQL command (statement batch) to execute.
    /// </summary>
    string CommandText { get; set; }

    /// <summary>
    ///   Executes the command and projects its result rows to PowerShell
    ///   objects.
    /// </summary>
    /// <param name="useSqlTypes">
    ///   <see langword="false"/> to project fields using CLR types from the
    ///     <see cref="System"/> namespace, such as <see cref="int"/>.
    ///   <see langword="true"/> to project fields using SQL types from the
    ///     <see cref="System.Data.SqlTypes"/> namespace, such as
    ///     <see cref="System.Data.SqlTypes.SqlInt32"/>.
    /// </param>
    /// <returns>
    ///   A sequence of objects created by executing the command and projecting
    ///   each result row to a PowerShell object.  If the command produces no
    ///   result rows, this method returns an empty sequence.
    /// </returns>
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
    IEnumerator<PSObject> ExecuteAndProjectToPSObjects(bool useSqlTypes = false);
}
