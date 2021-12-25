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

using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace PSql.Tests;

internal static class SecureStringHelpers
{
    public static SecureString Secure(this string s)
    {
        if (s is null)
            throw new ArgumentNullException(nameof(s));

        return new NetworkCredential("", s).SecurePassword;
    }

    public static SecureString GeneratePassword()
    {
        const string Chars  = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.,";
        const int    Length = 24;

        Span<byte> bytes = stackalloc byte[Length];
        RandomNumberGenerator.Fill(bytes);

        var password = new SecureString();

        for (var i = 0; i < bytes.Length; i++)
        {
            var n = bytes[i] & 0b0011_1111;
            var c = Chars[n];
            password.AppendChar(c);
        }

        return password;
    }
}
