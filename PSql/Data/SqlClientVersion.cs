// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Supported SqlClient versions.
/// </summary>
public enum SqlClientVersion
{
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
    ///   Microsoft.Data.SqlClient 3.x
    /// </summary>
    Mds3,

    /// <summary>
    ///   Microsoft.Data.SqlClient 4.x
    /// </summary>
    Mds4,

    /// <summary>
    ///   Microsoft.Data.SqlClient 5.0.x and 5.1.x
    /// </summary>
    Mds5,

    /// <summary>
    ///   Microsoft.Data.SqlClient 5.2.x
    /// </summary>
    Mds5_2,

    /// <summary>
    ///   The latest version supported by the current PSql module.
    /// </summary>
    Latest = Mds5_2
}
