// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;

namespace PSql;

internal static class PSCredentialExtensions
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this PSCredential? credential)
        => credential == null
        || credential == PSCredential.Empty;

    public static PSCredential? NullIfEmpty(this PSCredential? credential)
        => credential.IsNullOrEmpty() ? null : credential;
}
