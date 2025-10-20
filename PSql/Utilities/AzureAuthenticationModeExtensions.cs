// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

using static AzureAuthenticationMode;

internal static class AzureAuthenticationModeExtensions
{
    public static string? RenderForConnectionString(this AzureAuthenticationMode mode)
    {
        return mode switch
        {
            SqlPassword         => "Sql Password",
            AadPassword         => "Active Directory Password",
            AadIntegrated       => "Active Directory Integrated",
            AadInteractive      => "Active Directory Interactive",
            AadServicePrincipal => "Active Directory Service Principal",
            AadDeviceCodeFlow   => "Active Directory Device Code Flow",
            AadManagedIdentity  => "Active Directory Managed Identity",
            AadDefault          => "Active Directory Default",
            _                   => null,
        };
    }
}
