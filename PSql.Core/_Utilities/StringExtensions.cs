namespace PSql
{
    internal static class StringExtensions
    {
        internal static string NullIfEmpty(this string s)
            => string.IsNullOrEmpty(s) ? null : s;
    }
}
