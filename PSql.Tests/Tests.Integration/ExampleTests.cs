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
using NUnit.Framework;

namespace PSql.Tests.Integration
{
    using static FormattableString;

    [TestFixture]
    public class ExampleTests
    {
        [Test]
        public void Foo()
        {
            Execute(@"
                Invoke-Sql -Context $Context ""
                    SELECT X=1;
                ""
            ");
        }

        private static SqlServerContainer Server => IntegrationTestsSetup._container!;

        private readonly string Prelude = Invariant($@"
            $Credential = [PSCredential]::new(
                ""{Server.Credential.UserName}"",
                ""{Server.Credential.Password}""
            )
            $Context = New-SqlContext `
                -ServerName .,3341 `
                -Credential $Credential
        ").Unindent();

        private void Execute(string script)
        {
            ScriptExecutor.Execute(Prelude + script.Unindent());
        }
    }
}
