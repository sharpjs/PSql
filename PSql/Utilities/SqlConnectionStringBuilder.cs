// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace PSql;

/// <summary>
///   A simple, append-only connection string builder that supportes multiple
///   SqlClient versions.
/// </summary>
public readonly ref struct SqlConnectionStringBuilder
{
    private readonly StringBuilder _builder;

    /// <summary>
    ///   Initializes a new <see cref="SqlConnectionStringBuilder"/> instance
    ///   for the specified SqlClient version.
    /// </summary>
    /// <param name="version">
    ///   The SqlClient version against which the builder should maintain
    ///   compatibility.
    /// </param>
    public SqlConnectionStringBuilder(SqlClientVersion version = SqlClientVersion.Legacy)
    {
        _builder = new();
        Version  = version;
    }

    /// <summary>
    ///   Gets the SqlClient version against which the builder maintains
    ///   compatibility.
    /// </summary>
    public SqlClientVersion Version { get; }

    /// <summary>
    ///   Appends a property that specifies the name or network address of the
    ///   SQL Server, Azure SQL Database, or compatible product instance to
    ///   which to connect.
    /// </summary>
    public void AppendServerName(string? serverName)
    {
        Append("Data Source", serverName);
    }

    /// <summary>
    ///   Appends a property that specifies the name of the database to which
    ///   to connect.
    /// </summary>
    public void AppendDatabaseName(string? databaseName)
    {
        Append("Initial Catalog", databaseName);
    }

    /// <summary>
    ///   Appends a property that specifies to use the current user identity
    ///   for authentication.
    /// </summary>
    public void AppendIntegratedSecurity(bool enable)
    {
        Append("Integrated Security", enable);
    }

    /// <summary>
    ///   Appends a property that specifies the authentication mode.
    /// </summary>
    public void AppendAuthenticationMode(AzureAuthenticationMode mode)
    {
        Append("Authentication", mode.RenderForConnectionString());
    }

    /// <summary>
    ///   Appends properties that specify the username and password for
    ///   authentication.
    /// </summary>
    public void AppendCredential(NetworkCredential credential)
    {
        AppendUserName(credential.UserName);
        AppendPassword(credential.Password);
    }

    /// <summary>
    ///   Appends a property that specifies the username for authentication.
    /// </summary>
    public void AppendUserName(string userName)
    {
        Append("User ID", userName);
    }

    /// <summary>
    ///   Appends a property that specifies the password for authentication.
    /// </summary>
    public void AppendPassword(string password)
    {
        Append("Password", password);
    }

    /// <summary>
    ///   Appends a property that specifies whether security-sensitive
    ///   information such as password appears in a connection's
    ///   <see cref="DbConnection.ConnectionString"/> property after the
    ///   connection transitions to the open state.
    /// </summary>
    public void AppendPersistSecurityInfo(bool enable)
    {
        Append("Persist Security Info", enable);
    }

    /// <summary>
    ///   Appends a property that specifies whether to encrypt data sent
    ///   between between client and server if the server supports encryption.
    /// </summary>
    public void AppendEncrypt(bool enable)
    {
        Append("Encrypt", enable);
    }

    /// <summary>
    ///   Appends a property that specifies whether to skip verification of
    ///   the chain of trust of the certificate the server presents for
    ///   encryption.
    /// </summary>
    public void AppendTrustServerCertificate(bool enable)
    {
        Append(Version.GetTrustServerCertificateKey(), enable);
    }

    /// <summary>
    ///   Appends a property that specifies the duration to wait for a
    ///   connection to become established before terminating the attempt and
    ///   throwing an exception.
    /// </summary>
    public void AppendConnectTimeout(TimeSpan value)
    {
        Append("Connect Timeout", value.GetAbsoluteSecondsSaturatingInt32());
    }

    /// <summary>
    ///   Appends a property that specifies the client machine name to
    ///   associate with connections.
    /// </summary>
    public void AppendClientName(string? clientName)
    {
        Append("Workstation ID", clientName);
    }

    /// <summary>
    ///   Appends a property that specifies the client application name to
    ///   associate with connections.
    /// </summary>
    public void AppendApplicationName(string? applicationName)
    {
        Append("Application Name", applicationName);
    }

    /// <summary>
    ///   Appends a property that specifies the kind of operations the client
    ///   intends to perform using connections.
    /// </summary>
    public void AppendApplicationIntent(ApplicationIntent intent)
    {
        Append(Version.GetApplicationIntentKey(), intent.ToString());
    }

    /// <summary>
    ///   Appends a property that specifies whether connections will support
    ///   multiple active result sets.
    /// </summary>
    public void AppendMultipleActiveResultSets(bool enable)
    {
        Append(Version.GetMultipleActiveResultSetsKey(), enable);
    }

    /// <summary>
    ///   Appends a property that specifies whether connections will
    ///   participate in pooling.
    /// </summary>
    public void AppendPooling(bool enable)
    {
        Append("Pooling", enable);
    }

    /// <summary>
    ///   Returns the current connection string built by this instance.
    /// </summary>
    public override string ToString()
    {
        return _builder.ToString();
    }

    private void Append(string key, string? value)
    {
        AppendKey(key);
        AppendValue(value);
    }

    private void Append(string key, bool value)
    {
        AppendKey(key);
        _builder.Append(value ? "true" : "false");
    }

    private void Append(string key, int value)
    {
        AppendKey(key);
        _builder.Append(value.ToString(CultureInfo.InvariantCulture));
    }

    private void AppendKey(string key)
    {
        var builder = _builder;

        if (builder.Length > 0)
            builder.Append(';');

        builder
            .AppendEscaped(key, '=')
            .Append('=');
    }

    private void AppendValue(string? value)
    {
        if (value.IsNullOrEmpty())
            return;

        if (value.Contains('\0'))
            throw new FormatException("Value cannot contain a NUL (U+0000) character.");

        var builder = _builder;

        if (ShouldQuoteConnectionStringValueRegex.IsMatch(value))
            builder.AppendQuoted(value, '\"');
        else
            builder.Append(value);
    }

    // The characters that require a value to be quoted
    private static readonly Regex ShouldQuoteConnectionStringValueRegex = new(
        @"[""'=;\s\p{Cc}]",
        RegexOptions.Compiled |
        RegexOptions.CultureInvariant
    );
}
