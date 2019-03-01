using System;

namespace PSql
{
    internal static class TimeSpanExtensions
    {
        public static int GetTotalSecondsSaturatingInt32(this TimeSpan span)
        {
            long seconds = span.Ticks / TimeSpan.TicksPerSecond;

            return seconds > int.MaxValue ? int.MaxValue
                :  seconds < int.MinValue ? int.MinValue
                :  (int) seconds;
        }
    }
}
