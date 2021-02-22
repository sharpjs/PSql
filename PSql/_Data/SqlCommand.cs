/*
    Copyright 2021 Jeffrey Sharp

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
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;

namespace PSql
{
    /// <summary>
    ///   Represents a SQL command (statement batch) to execute against SQL
    ///   Server, Azure SQL Database, or compatible product.
    /// </summary>
    /// <remarks>
    ///   This type is a proxy for
    ///   <c>Microsoft.Data.SqlClient.SqlCommand.</c>
    /// </remarks>
    internal class SqlCommand : IDisposable
    {
        private readonly dynamic _command; // M.D.S.SqlCommand

        /// <summary>
        ///   Creates a new <see cref="SqlCommand"/> that can execute commands
        ///   on the specified connection.
        /// </summary>
        /// <param name="connection">
        ///   The connection on which to execute commands.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="connection"/> is <c>null</c>.
        /// </exception>
        internal SqlCommand(dynamic /*SqlConnection*/ connection)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));

            _command             = connection.CreateCommand();
            _command.Connection  = connection;
            _command.CommandType = CommandType.Text;
        }

        /// <summary>
        ///   Gets or sets the duration in seconds after which command
        ///   execution times out.  A value of <c>0</c> indicates no timeout:
        ///   a command is allowed to execute indefinitely.
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
        ///   <c>false</c> to project fields using CLR types from the
        ///     <see cref="System"/> namespace, such as <see cref="int"/>.
        ///   <c>true</c> to project fields using SQL types from the
        ///     <see cref="System.Data.SqlTypes"/> namespace, such as
        ///     <see cref="System.Data.SqlTypes.SqlInt32"/>.
        /// </param>
        internal IEnumerator<object> ExecuteAndProjectToObjects(SwitchParameter useSqlTypes)
        {
            return PSqlClient.Instance.ExecuteAndProject(
                _command, ObjectCreator, PropertySetter, useSqlTypes
            );
        }

        private static readonly Func<object>
            ObjectCreator = CreateObject;

        private static readonly Action<object, string, object?>
            PropertySetter = AddProperty;

        private static object CreateObject()
        {
            return new PSObject();
        }

        private static void AddProperty(object obj, string name, object? value)
        {
            var target   = (PSObject) obj;
            var property = new PSNoteProperty(name, value);

            target.Properties.Add(property);
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
}
