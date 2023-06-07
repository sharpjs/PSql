// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Kinds of operations that an application can declare it intends to
///   perform against a database.
/// </summary>
public enum ApplicationIntent // ~> M.D.S.ApplicationIntent
{
    /// <summary>
    ///   The application intends to perform reads and writes.
    /// </summary>
    ReadWrite,

    /// <summary>
    ///   The application intends to perform reads only.
    /// </summary>
    ReadOnly
}
