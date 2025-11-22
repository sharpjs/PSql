// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Tests.Unit;

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

    public static IEnumerable<Case> StringCases =
    [
        new Case("$null", null),
        new Case("''",    null),
        new Case("'a'",   "a" )
    ];

    public static IEnumerable<Case> SwitchCases =
    [
        new Case("",        true ),
        new Case(":$true",  true ),
        new Case(":$false", false)
    ];

    #region -ResourceGroupName

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ResourceGroupName_Set_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -Azure -ResourceGroupName {expression}
        "
        .ShouldOutput(
            new AzureSqlContext { ServerResourceGroupName = value }
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
            New-SqlContext -Azure -ResourceGroupName x | New-SqlContext -ResourceGroupName {expression}
        "
        .ShouldOutput(
            new AzureSqlContext { ServerResourceGroupName = value }
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
    #region -ServerResourceName

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ServerResourceName_Set_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -Azure -ServerResourceName {expression}
        "
        .ShouldOutput(
            new AzureSqlContext { ServerResourceName = value }
        );
    }

    [Test]
    public void ServerResourceName_Set_NonAzure()
    {
        @$"
            New-SqlContext -ServerResourceName x
        "
        .ShouldThrow<ParameterBindingException>(
            "Parameter set cannot be resolved using the specified named parameters."
        );
    }

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ServerResourceName_Override_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -Azure -ServerResourceName x | New-SqlContext -ServerResourceName {expression}
        "
        .ShouldOutput(
            new AzureSqlContext { ServerResourceName = value }
        );
    }

    [Test]
    public void ServerResourceName_Override_NonAzure()
    {
        @$"
            New-SqlContext | New-SqlContext -ServerResourceName x
        "
        .ShouldOutput(
            new PSWarning("The 'ServerResourceName' argument was ignored because the context is not an Azure SQL Database context."),
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
        new Case("$null",            null),
        new Case("1",     (ushort?)     1),
        new Case("65535", (ushort?) 65535)
    };

    public static IEnumerable<Case> InvalidPortCases = new[]
    {
        new Case("''",    @"Cannot validate argument on parameter 'ServerPort'. The value ""0"" is not a number between 1 and 65535, inclusive."),
        new Case("0",     @"Cannot validate argument on parameter 'ServerPort'. The value ""0"" is not a number between 1 and 65535, inclusive."),
        new Case("-1",    @"Cannot bind parameter 'ServerPort'. Cannot convert value """ +    @"-1"" to type ""System.UInt16"". Error: ""Value was either too large or too small for a UInt16."""),
        new Case("65536", @"Cannot bind parameter 'ServerPort'. Cannot convert value """ + @"65536"" to type ""System.UInt16"". Error: ""Value was either too large or too small for a UInt16.""")
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
                -ServerResourceName srv `
                -AuthenticationMode {expression}
        "
        .ShouldOutput(
            new AzureSqlContext
            {
                ServerResourceGroupName = "rg",
                ServerResourceName      = "srv",
                AuthenticationMode      = value
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
                -ServerResourceName srv `
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
                -ServerResourceName srv `
                -AuthenticationMode SqlPassword `
            | `
            New-SqlContext -AuthenticationMode {expression}
        "
        .ShouldOutput(
            new AzureSqlContext
            {
                ServerResourceGroupName = "rg",
                ServerResourceName      = "srv",
                AuthenticationMode      = value
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
                -ServerResourceName srv `
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
        new Case("([PSCredential]::Empty)", null),
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
                -ResourceGroupName  rg `
                -ServerResourceName srv `
                -EncryptionMode     Unverified
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
            New-SqlContext -Azure -ResourceGroupName rg -ServerResourceName srv `
            | `
            New-SqlContext -EncryptionMode Unverified
        "
        .ShouldOutput(
            new PSWarning("The 'EncryptionMode' argument was ignored because the context is an Azure SQL Database context."),
            new AzureSqlContext { ServerResourceGroupName = "rg", ServerResourceName = "srv" }
        );
    }

    #endregion
    #region -ReadOnlyIntent

    public static IEnumerable<Case> ReadOnlyIntentCases = new[]
    {
        new Case("",        ApplicationIntent.ReadOnly),
        new Case(":$true",  ApplicationIntent.ReadOnly),
        new Case(":$false", ApplicationIntent.ReadWrite)
    };

    [Test]
    [TestCaseSource(nameof(ReadOnlyIntentCases))]
    public void ReadOnlyIntent_Set_Valid(string expression, ApplicationIntent value)
    {
        @$"
            New-SqlContext -ReadOnlyIntent{expression}
        "
        .ShouldOutput(
            new SqlContext { ApplicationIntent = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(ReadOnlyIntentCases))]
    public void ReadOnlyIntent_Override_Valid(string expression, ApplicationIntent value)
    {
        @$"
            New-SqlContext -ReadOnlyIntent | New-SqlContext -ReadOnlyIntent{expression}
        "
        .ShouldOutput(
            new SqlContext { ApplicationIntent = value }
        );
    }

    #endregion
    #region -ClientName

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ClientName_Set_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -ClientName {expression}
        "
        .ShouldOutput(
            new SqlContext { ClientName = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ClientName_Override_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -ClientName x | New-SqlContext -ClientName {expression}
        "
        .ShouldOutput(
            new SqlContext { ClientName = value }
        );
    }

    #endregion
    #region -ApplicationName

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ApplicationName_Set_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -ApplicationName {expression}
        "
        .ShouldOutput(
            new SqlContext { ApplicationName = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(StringCases))]
    public void ApplicationName_Override_Valid(string expression, string? value)
    {
        @$"
            New-SqlContext -ApplicationName x | New-SqlContext -ApplicationName {expression}
        "
        .ShouldOutput(
            new SqlContext { ApplicationName = value }
        );
    }

    #endregion
    #region -ConnectTimeout

    public static IEnumerable<Case> ValidTimeoutCases = new[]
    {
        new Case(            "$null", null),
        new Case(         "00:00:00", new TimeSpan(              0)),
        new Case(         "00:00:05", new TimeSpan(    0, 0,  0, 5)),
        new Case(   "24855.03:14:07", new TimeSpan(24855, 3, 14, 7)),
        new Case(                "0", new TimeSpan(              0)),
        new Case(                "1", new TimeSpan(              1)),
        new Case(         "50000000", new TimeSpan(    0, 0,  0, 5)),
        new Case("21474836470000000", new TimeSpan(24855, 3, 14, 7))
    };

    public static IEnumerable<Case> InvalidTimeoutCases = new[]
    {
        new Case(                    "''", @"String '' was not recognized as a valid TimeSpan."),
        new Case(     "-00:00:00.0000001", @"The value """ +      @"-00:00:00.0000001"" is negative. Negative timeouts are not supported."),
        new Case("24855.03:14:07.0000001", @"The value """ + @"24855.03:14:07.0000001"" exceeds the maximum supported timeout, 24855.03:14:07."),
        new Case(                    "-1", @"The value """ +            @"-1.00:00:00"" is negative. Negative timeouts are not supported."),
        new Case(     "21474836470000001", @"The value """ + @"24855.03:14:07.0000001"" exceeds the maximum supported timeout, 24855.03:14:07."),
    };

    [Test]
    [TestCaseSource(nameof(ValidTimeoutCases))]
    public void ConnectTimeout_Set_Valid(string expression, TimeSpan? value)
    {
        @$"
            New-SqlContext -ConnectTimeout {expression}
        "
        .ShouldOutput(
            new SqlContext { ConnectTimeout = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(InvalidTimeoutCases))]
    public void ConnectTimeout_Invalid(string expression, string message)
    {
        @$"
            New-SqlContext -ConnectTimeout {expression}
        "
        .ShouldThrow<ParameterBindingException>(message);
    }

    [Test]
    [TestCaseSource(nameof(ValidTimeoutCases))]
    public void ConnectTimeout_Override_Valid(string expression, TimeSpan? value)
    {
        @$"
            New-SqlContext -ConnectTimeout 42 | New-SqlContext -ConnectTimeout {expression}
        "
        .ShouldOutput(
            new SqlContext { ConnectTimeout = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(InvalidTimeoutCases))]
    public void ConnectTimeout_Override_Invalid(string expression, string message)
    {
        @$"
            New-SqlContext -ConnectTimeout 1337 | New-SqlContext -ConnectTimeout {expression}
        "
        .ShouldThrow<ParameterBindingException>(message);
    }

    #endregion
    #region -ExposeCredentialInConnectionString

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void ExposeCredentialInConnectionString_Set_Valid(string expression, bool value)
    {
        @$"
            New-SqlContext -ExposeCredentialInConnectionString{expression}
        "
        .ShouldOutput(
            new SqlContext { ExposeCredentialInConnectionString = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void ExposeCredentialInConnectionString_Override_Valid(string expression, bool value)
    {
        @$"
            New-SqlContext -ExposeCredentialInConnectionString | New-SqlContext -ExposeCredentialInConnectionString{expression}
        "
        .ShouldOutput(
            new SqlContext { ExposeCredentialInConnectionString = value }
        );
    }

    #endregion
    #region -Pooling

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void Pooling_Set_Valid(string expression, bool value)
    {
        @$"
            New-SqlContext -Pooling{expression}
        "
        .ShouldOutput(
            new SqlContext { EnableConnectionPooling = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void Pooling_Override_Valid(string expression, bool value)
    {
        @$"
            New-SqlContext -Pooling | New-SqlContext -Pooling{expression}
        "
        .ShouldOutput(
            new SqlContext { EnableConnectionPooling = value }
        );
    }

    #endregion
    #region -MultipleActiveResultSets

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void MultipleActiveResultSets_Set_Valid(string expression, bool value)
    {
        @$"
            New-SqlContext -MultipleActiveResultSets{expression}
        "
        .ShouldOutput(
            new SqlContext { EnableMultipleActiveResultSets = value }
        );
    }

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void MultipleActiveResultSets_Override_Valid(string expression, bool value)
    {
        @$"
            New-SqlContext -MultipleActiveResultSets | New-SqlContext -MultipleActiveResultSets{expression}
        "
        .ShouldOutput(
            new SqlContext { EnableMultipleActiveResultSets = value }
        );
    }

    #endregion
    #region -Frozen

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void Frozen_Set_Valid(string expression, bool value)
    {
        // NOTE: Using -ServerName after -Frozen to verify that the freeze is
        // applied after setting other properties.

        var context = new SqlContext { ServerName = "a" };

        if (value)
            context.Freeze();

        @$"
            New-SqlContext -Frozen{expression} -ServerName a
        "
        .ShouldOutput(context);
    }

    [Test]
    [TestCaseSource(nameof(SwitchCases))]
    public void Frozen_Override_Valid(string expression, bool value)
    {
        // NOTE: Using -ServerName after -Frozen to verify that the freeze is
        // applied after setting other properties.

        var context = new SqlContext { ServerName = "a" };

        if (value)
            context.Freeze();

        @$"
            New-SqlContext -Frozen | New-SqlContext -Frozen{expression} -ServerName a
        "
        .ShouldOutput(context);
    }

    #endregion
}
