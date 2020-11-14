using System;
using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace PSql
{
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
}
