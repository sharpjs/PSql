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

#nullable enable

namespace PSql
{
    [Cmdlet(VerbsDiagnostic.Test, nameof(PSql))]
    public class TestPSqlCommand : Cmdlet2
    {
        protected override void ProcessRecord()
        {
            var client = PSqlClient.CreateObject("PSqlClient",
                new Action <string>                  (s => WriteHost(s)),
                new Action <string>                  (WriteWarning),
                new Action <object>                  (WriteObject),
                new Func   <object>                  (() => new PSObject()),
                new Action <object, string, object?> (AddProperty)
            );

            WriteHost("Hello.");
        }

        private static void AddProperty(object obj, string name, object? value)
        {
            ((PSObject) obj).Properties.Add(new PSNoteProperty(name, value));
        }
    }
}
