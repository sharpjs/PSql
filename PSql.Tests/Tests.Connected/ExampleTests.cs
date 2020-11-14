using System;
using NUnit.Framework;

namespace PSql.Tests.Connected
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

        private static SqlServer Server => ConnectedTestsSetup.SqlServer;

        private readonly string Prelude = Invariant($@"
            $Credential = [PSCredential]::new(
                ""{Server.Credential.UserName}"",
                ""{Server.Credential.Password}""
            )
            $Context = New-SqlContext `
                -ServerName .,{Server.Port} `
                -Credential $Credential
        ").Unindent();

        private void Execute(string script)
        {
            ScriptExecutor.Execute(Prelude + script.Unindent());
        }
    }
}
