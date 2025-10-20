// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Security;
using System.Security.Cryptography;

namespace PSql.Integration;

internal static class SecureStringHelpers
{
    public static SecureString GeneratePassword()
    {
        //                     0         1         2         3         4         5         6
        //                     01234567890123456789012345678901234567890123456789012345678901234
        const string Chars  = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,";
        const int    Length = 24;

        Span<byte> bytes = stackalloc byte[Length];
        RandomNumberGenerator.Fill(bytes);

        var password = new SecureString();

        for (var i = 0; i < bytes.Length; i++)
        {
            var n = bytes[i] & 63;
            var c = Chars[n];
            password.AppendChar(c);
        }

        password.MakeReadOnly();

        return password;
    }
}
