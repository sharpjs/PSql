using System;
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
        public void ResourceGroupName_Set_Valid(string expression, string? value)
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
        public void ResourceGroupName_Override_Valid(string expression, string? value)
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
        public void ServerName_Set_Valid(string expression, string? value)
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
        public void ServerName_Override_Valid(string expression, string? value)
        {
            @$"
                New-SqlContext -ServerName x | New-SqlContext -ServerName {expression}
            "
            .ShouldOutput(
                new SqlContext { ServerName = value }
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
        public void ServerPort_Set_Valid(string expression, ushort? value)
        {
            @$"
                New-SqlContext -ServerPort {expression}
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

        [Test]
        [TestCaseSource(nameof(ValidPortCases))]
        public void ServerPort_Override_Valid(string expression, ushort? value)
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
        public void ServerPort_Override_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -ServerPort 1337 | New-SqlContext -ServerPort {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        #endregion
        #region -InstanceName

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void InstanceName_Set_Valid(string expression, string? value)
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
        public void InstanceName_Override_Valid(string expression, string? value)
        {
            @$"
                New-SqlContext -InstanceName x | New-SqlContext -InstanceName {expression}
            "
            .ShouldOutput(
                new SqlContext { InstanceName = value }
            );
        }

        #endregion
        #region -DatabaseName

        [Test]
        [TestCaseSource(nameof(StringCases))]
        public void DatabaseName_Set_Valid(string expression, string? value)
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
        public void DatabaseName_Override_Valid(string expression, string? value)
        {
            @$"
                New-SqlContext -DatabaseName x | New-SqlContext -DatabaseName {expression}
            "
            .ShouldOutput(
                new SqlContext { DatabaseName = value }
            );
        }

        #endregion
        #region -AuthenticationMode

        public static IEnumerable<Case> ValidAuthenticationModeCases = new[]
        {
            new Case("0",                  AzureAuthenticationMode.Default),
            new Case("7",                  AzureAuthenticationMode.AadManagedIdentity),
            new Case("Default",            AzureAuthenticationMode.Default),
            new Case("AadManagedIdentity", AzureAuthenticationMode.AadManagedIdentity),
        };

        public static IEnumerable<Case> InvalidAuthenticationModeCases = new[]
        {
            new Case("$null", @"Cannot bind parameter 'AuthenticationMode'"),
            new Case("''",    @"Cannot bind parameter 'AuthenticationMode'"),
            new Case("-1",    @"Cannot bind parameter 'AuthenticationMode'"),
            new Case("8",     @"Cannot bind parameter 'AuthenticationMode'"),
            new Case("Wrong", @"Cannot bind parameter 'AuthenticationMode'"),
        };

        [Test]
        [TestCaseSource(nameof(ValidAuthenticationModeCases))]
        public void AuthenticationMode_Set_Valid(string expression, AzureAuthenticationMode value)
        {
            @$"
                New-SqlContext -Azure `
                    -ResourceGroupName  rg `
                    -ServerName         srv `
                    -AuthenticationMode {expression}
            "
            .ShouldOutput(
                new AzureSqlContext
                {
                    ResourceGroupName  = "rg",
                    ServerName         = "srv",
                    AuthenticationMode = value
                }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidAuthenticationModeCases))]
        public void AuthenticationMode_Set_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -Azure `
                    -ResourceGroupName  rg `
                    -ServerName         srv `
                    -AuthenticationMode {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        [Test]
        public void AuthenticationMode_Set_NonAzure()
        {
            @$"
                New-SqlContext -AuthenticationMode AadPassword
            "
            .ShouldThrow<ParameterBindingException>(
                "Parameter set cannot be resolved using the specified named parameters."
            );
        }

        [Test]
        [TestCaseSource(nameof(ValidAuthenticationModeCases))]
        public void AuthenticationMode_Override_Valid(string expression, AzureAuthenticationMode value)
        {
            @$"
                New-SqlContext -Azure `
                    -ResourceGroupName  rg `
                    -ServerName         srv `
                    -AuthenticationMode SqlPassword `
                | `
                New-SqlContext -AuthenticationMode {expression}
            "
            .ShouldOutput(
                new AzureSqlContext
                {
                    ResourceGroupName  = "rg",
                    ServerName         = "srv",
                    AuthenticationMode = value
                }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidAuthenticationModeCases))]
        public void AuthenticationMode_Override_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -Azure `
                    -ResourceGroupName  rg `
                    -ServerName         srv `
                    -AuthenticationMode SqlPassword `
                | `
                New-SqlContext -AuthenticationMode {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        [Test]
        public void AuthenticationMode_Override_NonAzure()
        {
            @$"
                New-SqlContext | New-SqlContext -AuthenticationMode AadServicePrincipal
            "
            .ShouldOutput(
                new PSWarning("The 'AuthenticationMode' argument was ignored because the context is not an Azure SQL Database context."),
                new SqlContext()
            );
        }

        #endregion
        #region -Credential

        public static IEnumerable<Case> ValidCredentialCases = new[]
        {
            new Case("$null",                   null),
            new Case("([PSCredential]::Empty)", PSCredential.Empty),
            new Case("$Credential",             new PSCredential("a", "p".Secure())),
        };

        public static IEnumerable<Case> InvalidCredentialCases = new[]
        {
            new Case("''",       @"Cannot process argument transformation on parameter 'Credential'. A command that prompts the user failed because the host program or the command type does not support user interaction."),
            new Case("username", @"Cannot process argument transformation on parameter 'Credential'. A command that prompts the user failed because the host program or the command type does not support user interaction."),
        };

        [Test]
        [TestCaseSource(nameof(ValidCredentialCases))]
        public void Credential_Set_Valid(string expression, PSCredential? value)
        {
            @$"
                $Password   = ConvertTo-SecureString p -AsPlainText
                $Credential = New-Object PSCredential a, $Password
                New-SqlContext -Credential {expression}
            "
            .ShouldOutput(
                new SqlContext { Credential = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidCredentialCases))]
        public void Credential_Set_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -Credential {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        [Test]
        [TestCaseSource(nameof(ValidCredentialCases))]
        public void Credential_Override_Valid(string expression, PSCredential? value)
        {
            @$"
                $Password    = ConvertTo-SecureString p -AsPlainText
                $Credential  = New-Object PSCredential a, $Password
                $Credential2 = New-Object PSCredential b, $Password
                New-SqlContext -Credential $Credential2 | New-SqlContext -Credential {expression}
            "
            .ShouldOutput(
                new SqlContext { Credential = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidCredentialCases))]
        public void Credential_Override_Invalid(string expression, string message)
        {
            @$"
                $Password    = ConvertTo-SecureString p -AsPlainText
                $Credential2 = New-Object PSCredential b, $Password
                New-SqlContext -Credential $Credential2 | New-SqlContext -Credential {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        #endregion
        #region -EncryptionMode

        public static IEnumerable<Case> ValidEncryptionModeCases = new[]
        {
            new Case("0",       EncryptionMode.Default),
            new Case("3",       EncryptionMode.Full),
            new Case("Default", EncryptionMode.Default),
            new Case("Full",    EncryptionMode.Full),
        };

        public static IEnumerable<Case> InvalidEncryptionModeCases = new[]
        {
            new Case("$null", @"Cannot bind parameter 'EncryptionMode'"),
            new Case("''",    @"Cannot bind parameter 'EncryptionMode'"),
            new Case("-1",    @"Cannot bind parameter 'EncryptionMode'"),
            new Case("4",     @"Cannot bind parameter 'EncryptionMode'"),
            new Case("Wrong", @"Cannot bind parameter 'EncryptionMode'"),
        };

        [Test]
        [TestCaseSource(nameof(ValidEncryptionModeCases))]
        public void EncryptionMode_Set_Valid(string expression, EncryptionMode value)
        {
            @$"
                New-SqlContext -EncryptionMode {expression}
            "
            .ShouldOutput(
                new SqlContext { EncryptionMode = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidEncryptionModeCases))]
        public void EncryptionMode_Set_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -EncryptionMode {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        [Test]
        public void EncryptionMode_Set_Azure()
        {
            @$"
                New-SqlContext -Azure `
                    -ResourceGroupName rg `
                    -ServerName        srv `
                    -EncryptionMode    Unverified
            "
            .ShouldThrow<ParameterBindingException>(
                "Parameter set cannot be resolved using the specified named parameters."
            );
        }

        [Test]
        [TestCaseSource(nameof(ValidEncryptionModeCases))]
        public void EncryptionMode_Override_Valid(string expression, EncryptionMode value)
        {
            @$"
                New-SqlContext -EncryptionMode None | New-SqlContext -EncryptionMode {expression}
            "
            .ShouldOutput(
                new SqlContext { EncryptionMode = value }
            );
        }

        [Test]
        [TestCaseSource(nameof(InvalidEncryptionModeCases))]
        public void EncryptionMode_Override_Invalid(string expression, string message)
        {
            @$"
                New-SqlContext -EncryptionMode None | New-SqlContext -EncryptionMode {expression}
            "
            .ShouldThrow<ParameterBindingException>(message);
        }

        [Test]
        public void EncryptionMode_Override_Azure()
        {
            @$"
                New-SqlContext -Azure -ResourceGroupName rg -ServerName srv `
                | `
                New-SqlContext -EncryptionMode Unverified
            "
            .ShouldOutput(
                new PSWarning("The 'EncryptionMode' argument was ignored because the context is an Azure SQL Database context."),
                new AzureSqlContext { ResourceGroupName = "rg", ServerName = "srv" }
            );
        }

        #endregion

        public static Case String(string expression, string? value)
            => new(expression, value);

        public static Case UInt16(string expression, ushort? value)
            => new(expression, value);

        public static Case Invalid(string expression, string message)
            => new(expression, string.Format(message, expression));

        public static IEnumerable<Case> StringCases = new[]
        {
            String("$null", null),
            String("''",    null),
            String("'a'",   "a" )
        };
    }
}
