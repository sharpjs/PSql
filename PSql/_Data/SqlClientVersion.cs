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

/// <summary>
///   Supported SqlClient versions.
/// </summary>
public enum SqlClientVersion
{
    /*
        Connection string history:

        MDS 1.0
        - Authentication: AadPassword                   (all)
        - Authentication: AadIntegrated, AadInteractive (netfx)

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
    */

    /// <summary>
    ///   System.Data.SqlClient
    /// </summary>
    Legacy,

    /// <summary>
    ///   Microsoft.Data.SqlClient 1.0.x
    /// </summary>
    Mds1,

    /// <summary>
    ///   Microsoft.Data.SqlClient 1.1.x
    /// </summary>
    Mds1_1,

    /// <summary>
    ///   Microsoft.Data.SqlClient 2.0.x
    /// </summary>
    Mds2,

    /// <summary>
    ///   Microsoft.Data.SqlClient 2.1.x
    /// </summary>
    Mds2_1,

    /// <summary>
    ///   Microsoft.Data.SqlClient 3.0.x
    /// </summary>
    Mds3,

    /// <summary>
    ///   Microsoft.Data.SqlClient 4.0.x
    /// </summary>
    Mds4,
}
