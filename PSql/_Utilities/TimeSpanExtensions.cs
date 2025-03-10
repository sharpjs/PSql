// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

internal static class TimeSpanExtensions
{
    public static int GetAbsoluteSecondsSaturatingInt32(this TimeSpan span)
    {
        long seconds = span.Ticks / TimeSpan.TicksPerSecond;

        if (seconds < 0)
            seconds = -seconds;

        return seconds > int.MaxValue
            ? int.MaxValue
            : (int) seconds;
    }
}
