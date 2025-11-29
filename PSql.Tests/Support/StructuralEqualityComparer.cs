// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace PSql.Tests;

using PropertyMap = SortedDictionary<string, PropertyInfo>;

internal class StructuralEqualityComparer : IEqualityComparer<object?>
{
    public static StructuralEqualityComparer Instance { get; }
        = new StructuralEqualityComparer();

    private static readonly ConcurrentDictionary<Type, PropertyMap>
        PropertiesCache = new();

    public new bool Equals(object? x, object? y)
    {
        return new Comparison().Equals(x, y);
    }

    private class Comparison : IEqualityComparer<object?>
    {
        private readonly Dictionary<object, int> _xVisited = new();
        private readonly Dictionary<object, int> _yVisited = new();

        public new bool Equals(object? x, object? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            return x switch
            {
                null              => y is null,
                bool           x_ => y is bool              y_ && x_ == y_,
                sbyte          x_ => y is sbyte             y_ && x_ == y_,
                byte           x_ => y is byte              y_ && x_ == y_,
                short          x_ => y is short             y_ && x_ == y_,
                ushort         x_ => y is ushort            y_ && x_ == y_,
                int            x_ => y is int               y_ && x_ == y_,
                uint           x_ => y is uint              y_ && x_ == y_,
                long           x_ => y is long              y_ && x_ == y_,
                ulong          x_ => y is ulong             y_ && x_ == y_,
                nint           x_ => y is nint              y_ && x_ == y_,
                nuint          x_ => y is nuint             y_ && x_ == y_,
                float          x_ => y is float             y_ && x_ == y_,
                double         x_ => y is double            y_ && x_ == y_,
                char           x_ => y is char              y_ && x_ == y_,
                string         x_ => y is string            y_ && x_ == y_,
                DateOnly       x_ => y is DateOnly          y_ && x_ == y_,
                TimeOnly       x_ => y is TimeOnly          y_ && x_ == y_,
                DateTime       x_ => y is DateTime          y_ && x_ == y_,
                DateTimeOffset x_ => y is DateTimeOffset    y_ && x_ == y_,
                IEnumerable    x_ => y is IEnumerable       y_ && EqualsCore(x_, y_),
                { }               => y is { }               y_ && EqualsCore(x,  y ),
            };
        }

        private bool EqualsCore(IEnumerable x, IEnumerable y)
        {
            var xEnumerator = null as IEnumerator;
            var yEnumerator = null as IEnumerator;

            try
            {
                xEnumerator = x.GetEnumerator();
                yEnumerator = y.GetEnumerator();

                for (;;)
                {
                    var xDone = !xEnumerator.MoveNext();
                    var yDone = !yEnumerator.MoveNext();
                    if (xDone)
                        return yDone;

                    var xCurrent = xEnumerator.Current;
                    var yCurrent = yEnumerator.Current;
                    if (!Equals(xCurrent, yCurrent))
                        return false;
                }
            }
            finally
            {
                (xEnumerator as IDisposable)?.Dispose();
                (yEnumerator as IDisposable)?.Dispose();
            }
        }

        private bool EqualsCore(object x, object y)
        {
            var xIsCycle = IsCycle(_xVisited, x, out var xId);
            var yIsCycle = IsCycle(_yVisited, y, out var yId);
            if (xIsCycle | yIsCycle)
                return xIsCycle & yIsCycle && xId == yId;

            var xProperties = GetProperties(x);
            var yProperties = GetProperties(y);

            if (xProperties.Count != yProperties.Count)
                return false;

            foreach (var xProperty in xProperties.Values)
            {
                if (!yProperties.TryGetValue(xProperty.Name, out var yProperty))
                    return false;

                var xValue = xProperty.GetValue(x);
                var yValue = yProperty.GetValue(y);

                if (!Equals(xValue, yValue))
                    return false;
            }

            return true;
        }

        int IEqualityComparer<object?>.GetHashCode(object? obj)
            => throw new NotSupportedException();
    }

    public int GetHashCode([DisallowNull] object? obj)
    {
        return new HashOperation().GetHashCode(obj);
    }

    private class HashOperation : IEqualityComparer<object?>
    {
        private readonly Dictionary<object, int> _visited = new();

        public int GetHashCode([DisallowNull] object? obj)
        {
            unchecked
            {
                return obj switch
                {
                    null             => (int) 0xBE74AF54,
                    bool           o => (int) 0x14AE1F83 ^ o.GetHashCode(),
                    sbyte          o => (int) 0x64988A44 ^ o.GetHashCode(),
                    byte           o => (int) 0x2725CD93 ^ o.GetHashCode(),
                    short          o => (int) 0x92EDE58F ^ o.GetHashCode(),
                    ushort         o => (int) 0x708C3042 ^ o.GetHashCode(),
                    int            o => (int) 0x7FC7F057 ^ o.GetHashCode(),
                    uint           o => (int) 0xFAF35C06 ^ o.GetHashCode(),
                    long           o => (int) 0xC2A9D144 ^ o.GetHashCode(),
                    ulong          o => (int) 0xAA4242B2 ^ o.GetHashCode(),
                    nint           o => (int) 0x5E1BFD69 ^ o.GetHashCode(),
                    nuint          o => (int) 0xE30F3AA3 ^ o.GetHashCode(),
                    float          o => (int) 0x390C980D ^ o.GetHashCode(),
                    double         o => (int) 0xC1DACA5C ^ o.GetHashCode(),
                    char           o => (int) 0x13B4011B ^ o.GetHashCode(),
                    string         o => (int) 0xDB928E18 ^ o.GetHashCode(),
                    DateOnly       o => (int) 0x3EC3DEBC ^ o.GetHashCode(),
                    TimeOnly       o => (int) 0x2520BCB7 ^ o.GetHashCode(),
                    DateTime       o => (int) 0x6FFCB343 ^ o.GetHashCode(),
                    DateTimeOffset o => (int) 0x4CDD59E8 ^ o.GetHashCode(),
                    IEnumerable    o => (int) 0x11BAD477 ^ GetHashCodeCore(o),
                    { }            o => (int) 0xD3258C2F ^ GetHashCodeCore(o),
                };
            }
        }

        private int GetHashCodeCore(IEnumerable obj)
        {
            var hash = new HashCode();

            foreach (var item in obj)
                hash.Add(item, this);

            return hash.ToHashCode();
        }

        private int GetHashCodeCore(object obj)
        {
            if (IsCycle(_visited, obj, out var id))
                return id.GetHashCode();

            var hash = new HashCode();

            foreach (var name in GetProperties(obj).Keys)
                hash.Add(name);

            foreach (var property in GetProperties(obj).Values)
                hash.Add(property.GetValue(obj), this);

            return hash.ToHashCode();
        }

        bool IEqualityComparer<object?>.Equals(object? x, object? y)
            => throw new NotSupportedException();
    }

    private static PropertyMap GetProperties(object obj)
    {
        return PropertiesCache.GetOrAdd(obj.GetType(), GetPropertiesCore);
    }

    private static PropertyMap GetPropertiesCore(Type type)
    {
        var properties = new PropertyMap();

        foreach (var property in type.GetProperties())
            if (IsComparable(property))
                properties[property.Name] = property;

        return properties;
    }

    private static bool IsComparable(PropertyInfo property)
    {
        return property.CanRead                             // exclude write-only properties
            && property.GetIndexParameters().Length is 0;   // exclude indexers
    }

    private static bool IsCycle(Dictionary<object, int> visited, object obj, out int id)
    {
        if (visited.TryGetValue(obj, out id))
            return true;

        visited[obj] = id = visited.Count;
        return false;
    }
}
