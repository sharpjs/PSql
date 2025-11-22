// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

internal static class ObjectExtensions
{
    [return: NotNullIfNotNull(nameof(obj))]
    public static object? UnwrapPSObject(this object? obj)
    {
        return obj is PSObject psObject ? psObject.BaseObject : obj;
    }
}
