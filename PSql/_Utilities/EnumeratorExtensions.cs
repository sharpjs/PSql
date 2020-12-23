using System.Collections.Generic;

namespace PSql
{
    internal static class EnumeratorExtensions
    {
        public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enumerator)
            => enumerator;
    }
}
