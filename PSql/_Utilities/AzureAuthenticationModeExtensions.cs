/*
    Copyright 2021 Jeffrey Sharp

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
