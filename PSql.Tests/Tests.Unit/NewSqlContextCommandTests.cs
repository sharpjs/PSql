using System.Collections.Generic;
using System.Management.Automation;
using NUnit.Framework;

namespace PSql.Tests.Unit
{
    using Case = TestCaseData;

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

        #region -ResourceGroupName

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void ResourceGroupName_Set(string expression, string? value)
        {
            @$"
                New-SqlContext -Azure -ResourceGroupName {expression} -ServerName x
            "
            .ShouldOutput(
                new AzureSqlContext { ResourceGroupName = value, ServerName = "x" }
            );
        }

        [Test]
        public void ResourceGroupName_Set_NonAzure()
        {
            @$"
                New-SqlContext -ResourceGroupName x
            "
            .ShouldThrow<ParameterBindingException>(
                "Parameter set cannot be resolved using the specified named parameters."
            );
        }

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void ResourceGroupName_Override(string expression, string? value)
        {
            @$"
                New-SqlContext -Azure -ResourceGroupName x -ServerName x | New-SqlContext -ResourceGroupName {expression}
            "
            .ShouldOutput(
                new AzureSqlContext { ResourceGroupName = value, ServerName = "x" }
            );
        }

        [Test]
        public void ResourceGroupName_Override_NonAzure()
        {
            @$"
                New-SqlContext | New-SqlContext -ResourceGroupName x
            "
            .ShouldOutput(
                new PSWarning("The 'ResourceGroupName' argument was ignored because the context is not an Azure SQL Database context."),
                new SqlContext()
            );
        }

        #endregion
        #region -ServerName

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void ServerName_Set(string expression, string? value)
        {
            @$"
                New-SqlContext -ServerName {expression}
            "
            .ShouldOutput(
                new SqlContext { ServerName = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void ServerName_Override(string expression, string? value)
        {
            @$"
                New-SqlContext -ServerName x | New-SqlContext -ServerName {expression}
            "
            .ShouldOutput(
                new SqlContext { ServerName = value }
            );
        }

        #endregion
        #region -DatabaseName

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void DatabaseName_Set(string expression, string? value)
        {
            @$"
                New-SqlContext -DatabaseName {expression}
            "
            .ShouldOutput(
                new SqlContext { DatabaseName = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void DatabaseName_Override(string expression, string? value)
        {
            @$"
                New-SqlContext -DatabaseName x | New-SqlContext -DatabaseName {expression}
            "
            .ShouldOutput(
                new SqlContext { DatabaseName = value }
            );
        }

        #endregion
        #region -InstanceName

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void InstanceName_Set(string expression, string? value)
        {
            @$"
                New-SqlContext -InstanceName {expression}
            "
            .ShouldOutput(
                new SqlContext { InstanceName = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void InstanceName_Override(string expression, string? value)
        {
            @$"
                New-SqlContext -InstanceName x | New-SqlContext -InstanceName {expression}
            "
            .ShouldOutput(
                new SqlContext { InstanceName = value }
            );
        }

        #endregion
        #region -ServerPort

        public static IEnumerable<Case> ValidPortCases = new[]
        {
            UInt16("$null",  null),
            UInt16("1",         1),
            UInt16("65535", 65535)
        };

        public static IEnumerable<Case> InvalidPortCases = new[]
        {
            Invalid("''",    @"Cannot validate argument on parameter 'ServerPort'. The value ""0"" is not a positive number."),
            Invalid("0",     @"Cannot validate argument on parameter 'ServerPort'. The value ""0"" is not a positive number."),
            Invalid("-1",    @"Cannot bind parameter 'ServerPort'. Cannot convert value ""{0}"" to type ""System.UInt16"". Error: ""Value was either too large or too small for a UInt16."""),
            Invalid("65536", @"Cannot bind parameter 'ServerPort'. Cannot convert value ""{0}"" to type ""System.UInt16"". Error: ""Value was either too large or too small for a UInt16.""")
        };

        [Test]
        [TestCaseSource(nameof(ValidPortCases))]
        public void ServerPort_Set(string expression, ushort? value)
        {
            @$"
                New-SqlContext -ServerPort {expression}
            "
            .ShouldOutput(
                new SqlContext { ServerPort = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(ValidPortCases))]
        public void ServerPort_Override(string expression, ushort? value)
        {
            @$"
                New-SqlContext -ServerPort 42 | New-SqlContext -ServerPort {expression}
            "
            .ShouldOutput(
                new SqlContext { ServerPort = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidPortCases))]
        public void ServerPort_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -ServerPort {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        #endregion

        public static Case String(string expression, string? value)
            => new Case(expression, value);

        public static Case UInt16(string expression, ushort? value)
            => new Case(expression, value);

        public static Case Invalid(string expression, string message)
            => new Case(expression, string.Format(message, expression));

        public static IEnumerable<Case> StringCases = new[]
        {
            String("$null", null),
            String("''",    null),
            String("'a'",   "a" )
        };
    }
}
