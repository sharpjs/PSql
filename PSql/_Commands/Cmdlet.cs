/*
    Copyright 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PSql
{
    /// <summary>
    ///   Base class for PSql cmdlets.
    /// </summary>
    public abstract class Cmdlet : System.Management.Automation.Cmdlet
    {
        private static readonly string[]
            HostTag = { "PSHOST" };

        public void WriteHost(string message,
            bool          newLine         = true,
            ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null)
        {
            // Technique learned from PSv5+ Write-Host implementation, which
            // works by sending specially-marked messages to the information
            // stream.
            //
            // https://github.com/PowerShell/PowerShell/blob/v7.0.3/src/Microsoft.PowerShell.Commands.Utility/commands/utility/WriteConsoleCmdlet.cs

            var data = new HostInformationMessage
            {
                Message   = message,
                NoNewLine = !newLine
            };

            if (foregroundColor.HasValue || backgroundColor.HasValue)
            {
                try
                {
                    data.ForegroundColor = foregroundColor;
                    data.BackgroundColor = backgroundColor;
                }
                catch (HostException)
                {
                    // Host is non-interactive or does not support colors.
                }
            }

            WriteInformation(data, HostTag);
        }

        /// <summary>
        ///   Returns the specified shared <see cref="SqlConnection"/> instance
        ///   if provided, or creates a new, owned instance using the specified
        ///   context and database name.
        /// </summary>
        /// <param name="connection">
        ///   The shared connection.  If provided, the method returns this
        ///   connection.
        /// </param>
        /// <param name="context">
        ///   An object containing information necessary to connect to a SQL
        ///   Server or compatible database if <paramref name="connection"/> is
        ///   <c>null</c>.  If not provided, the method will use a context with
        ///   default property values as necessary.
        /// </param>
        /// <param name="databaseName">
        ///   The name of the database to which to connect if
        ///   <paramref name="connection"/> is <c>null</c>.  If not provided,
        ///   the method connects to the default database for the context.
        /// </param>
        /// <returns>
        ///   A tuple consisting of the resulting connection and a value that
        ///   indicates whether the caller owns the connection and must ensure
        ///   its disposal.  If <paramref name="connection"/> is provided, the
        ///   method returns that connection and <c>false</c> (shared).
        ///   Otherwise, the method creates a new connection as specified by
        ///   <paramref name="context"/> and <paramref name="databaseName"/>
        ///   and returns the connection and <c>true</c> (owned).
        /// </returns>
        protected (SqlConnection, bool owned) EnsureConnection(
            SqlConnection? connection,
            SqlContext?    context,
            string?        databaseName)
        {
            if (connection != null)
                return (connection, false);

            context ??= new SqlContext { DatabaseName = databaseName };

            return (new SqlConnection(context, this), true);
        }
    }
}
