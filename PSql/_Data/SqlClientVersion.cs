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
    ///   Microsoft.Data.SqlClient 5.x
    /// </summary>
    Mds5,

    /// <summary>
    ///   The latest version supported by the current code.
    /// </summary>
    Latest = Mds5
}
