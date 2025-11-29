// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

internal static class PSCredentialExtensions
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this PSCredential? credential)
        => credential is null
        || credential == PSCredential.Empty;

    public static PSCredential? NullIfEmpty(this PSCredential? credential)
        => credential.IsNullOrEmpty() ? null : credential;
}
