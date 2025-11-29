// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

using static AzureAuthenticationMode;

[TestFixture]
public class AzureAuthenticationModeExtensionsTests
{
    [Test]
    [TestCase(SqlPassword,         "Sql Password"                      )]
    [TestCase(AadPassword,         "Active Directory Password"         )]
    [TestCase(AadIntegrated,       "Active Directory Integrated"       )]
    [TestCase(AadInteractive,      "Active Directory Interactive"      )]
    [TestCase(AadServicePrincipal, "Active Directory Service Principal")]
    [TestCase(AadDeviceCodeFlow,   "Active Directory Device Code Flow" )]
    [TestCase(AadManagedIdentity,  "Active Directory Managed Identity" )]
    [TestCase(AadDefault,          "Active Directory Default"          )]
    [TestCase(AadWorkloadIdentity, "Active Directory Workload Identity")]
    [TestCase(-1,                  null                                )]
    public void RenderForConnectionString(AzureAuthenticationMode mode, string? value)
    {
        mode.RenderForConnectionString().ShouldBe(value);
    }
}
