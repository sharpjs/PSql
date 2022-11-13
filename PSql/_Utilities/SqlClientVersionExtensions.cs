/*
    Copyright 2022 Jeffrey Sharp

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
using static SqlClientVersion;

internal static class SqlClientVersionExtensions
{
    /*
        Connection string history, distilled from:
        https://github.com/dotnet/SqlClient/tree/main/release-notes

        Legacy
        - [NOT SUPPORTED] Authentication
        - When username and password are provided, behaves as if
          Authentication: SqlPassword were specified.

        MDS 1.0 (netfx)
        - Authentication: SqlPassword, AadPassword, AadIntegrated, AadInteractive

        MDS 1.0 (others)
        - Authentication: AadPassword

        MDS 1.1
        - Attestation Protocol
        - Enclave Attestation Url

        MDS 2.0:
        - Authentication: AadIntegrated, AadInteractive, AadServicePrincipal
        - Application Intent                (was: ApplicationIntent)
        - Connect Retry Count               (was: ConnectRetryCount)
        - Connect Retry Interval            (was: ConnectRetryInterval)
        - Pool Blocking Period              (was: PoolBlockingPeriod)
        - Multiple Active Result Sets       (was: MultipleActiveResultSets)
        - Multi Subnet Failover             (was: MultiSubnetFailover)
        - Transparent Network IP Resolution (was: TransparentNetworkIPResolution)
        - Trust Server Certificate          (was: TrustServerCertificate)

        MDS 2.1
        - Authentication: AadDeviceCodeFlow, AadManagedIdentity
        - Command Timeout

        MDS 3.0
        - Authentication: AadDefault
        - The User ID connection property now requires a client id instead of
          an object id for user-assigned managed identity.

        MDS 4.0
        - Encrypt: true by default
        - Authentication: AadIntegrated (allows User ID)
        - [REMOVED] Asynchronous Processing

        MDS 5.0
        - Encrypt: Optional, Mandatory, Strict
        - TDS 8
    */

    public static bool SupportsAuthenticationMode(
        this SqlClientVersion version, AzureAuthenticationMode mode)
        => mode switch
        {
            Default             => version >= Mds1,
            SqlPassword         => version >= Mds1,
            AadPassword         => version >= Mds1,
            AadIntegrated       => version >= Mds1,
            AadInteractive      => version >= Mds1,
            AadServicePrincipal => version >= Mds2,
            AadDeviceCodeFlow   => version >= Mds2_1,
            AadManagedIdentity  => version >= Mds2_1,
            AadDefault          => version >= Mds3,
            _                   => false,
        };

    public static bool GetDefaultEncrypt(this SqlClientVersion version)
        => version >= Mds4;

    public static string GetApplicationIntentKey(this SqlClientVersion version)
        => version >= Mds2
        ? "Application Intent"
        : "ApplicationIntent";

    public static string GetMultipleActiveResultSetsKey(this SqlClientVersion version)
        => version >= Mds2
        ? "Multiple Active Result Sets"
        : "MultipleActiveResultSets";

    public static string GetTrustServerCertificateKey(this SqlClientVersion version)
        => version >= Mds2
        ? "Trust Server Certificate"
        : "TrustServerCertificate";
}
