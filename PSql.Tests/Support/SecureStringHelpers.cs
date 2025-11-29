// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;
using System.Security;

namespace PSql.Tests;

internal static class SecureStringHelpers
{
    public static SecureString Secure(this string s)
    {
        if (s is null)
            throw new ArgumentNullException(nameof(s));

        return new NetworkCredential("", s).SecurePassword;
    }
}
