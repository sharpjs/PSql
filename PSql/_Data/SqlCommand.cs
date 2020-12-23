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
using System.Collections.Generic;
using System.Data;
using System.Management.Automation;

namespace PSql
{
    internal class SqlCommand : IDisposable // ~> M.D.S.SqlCommand
    {
        private readonly dynamic _command;

        public SqlCommand(dynamic connection, Cmdlet cmdlet)
        {
            if (connection is null)
                throw new ArgumentNullException(nameof(connection));
            if (cmdlet is null)
                throw new ArgumentNullException(nameof(cmdlet));

            _command             = connection.CreateCommand();
            _command.Connection  = connection;
            _command.CommandType = CommandType.Text;
        }

        public int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public string CommandText
        {
            get => _command.CommandText;
            set => _command.CommandText = value;
        }

        internal IEnumerator<object> ExecuteAndProjectToPSObjects(SwitchParameter useSqlTypes)
        {
            return PSqlClient.Instance.ExecuteAndProject(
                _command,
                new Func   <object>                  (() => new PSObject()),
                new Action <object, string, object?> ((o, n, v) => ((PSObject) o).Properties.Add(new PSNoteProperty(n, v))),
                useSqlTypes
            );
        }

        protected virtual void Dispose(bool managed)
        {
            if (!managed)
                return;

            _command.Dispose();
        }

        public void Dispose()
        {
            Dispose(managed: true);
            GC.SuppressFinalize(this);
        }
    }
}
