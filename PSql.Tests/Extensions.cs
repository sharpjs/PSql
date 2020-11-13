using System;
using System.Security;
using System.Security.Cryptography;

namespace PSql
{
    internal static class Extensions
    {
        public static SecureString Secure(this string self)
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            var secure = new SecureString();

            try
            {
                foreach (var c in self)
                    secure.AppendChar(c);

                return secure;
            }
            catch
            {
                secure.Dispose();
                throw;
            }
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
