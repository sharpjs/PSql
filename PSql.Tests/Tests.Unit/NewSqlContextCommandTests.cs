using System.Management.Automation;
using NUnit.Framework;

namespace PSql.Tests.Unit
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class NewSqlContextCommandTests
    {
        [Test]
        public void Default()
        {
            @"
                New-SqlContext
            "
            .ShouldOutput(
                new SqlContext()
            );
        }

        [Test]
        public void ServerName_Set_Null()
        {
            @"
                New-SqlContext -ServerName $null
            "
            .ShouldOutput(
                new SqlContext { }
            );
        }

        [Test]
        public void ServerName_Set_Empty()
        {
            @"
                New-SqlContext -ServerName ''
            "
            .ShouldThrow<ParameterBindingException>(
                "Cannot bind argument to parameter 'ServerName' because it is an empty string."
            );
        }

        [Test]
        public void ServerName_Set_NotNull()
        {
            @"
                New-SqlContext -ServerName a
            "
            .ShouldOutput(
                new SqlContext { ServerName = "a" }
            );
        }

        [Test]
        public void ServerName_Override_Null()
        {
            @"
                New-SqlContext -ServerName a | New-SqlContext -ServerName $null
            "
            .ShouldOutput(
                new SqlContext { }
            );
        }

        [Test]
        public void ServerName_Override_NotNull()
        {
            @"
                New-SqlContext -ServerName a | New-SqlContext -ServerName b
            "
            .ShouldOutput(
                new SqlContext { ServerName = "b" }
            );
        }
    }
}
