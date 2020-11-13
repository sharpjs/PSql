using System;
using System.Security;

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
    }
}
