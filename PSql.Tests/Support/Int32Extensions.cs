// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

internal static class Int32Extensions
{
    public static TimeSpan Seconds(this int seconds)
        => TimeSpan.FromSeconds(seconds);

    public static TimeSpan Hours(this int hours)
        => TimeSpan.FromHours(hours);
}
