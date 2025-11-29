// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

internal static class PSObjectExtensions
{
    internal static void
        ShouldHaveProperties(
            this PSObject?                      obj,
            Action<IEnumerator<PSPropertyInfo>> assertion
        )
    {
        obj.ShouldNotBeNull();

        using var properties = obj!.Properties.GetEnumerator();

        assertion(properties);

        properties.MoveNext().ShouldBeFalse();
    }

    internal static IEnumerator<PSPropertyInfo>
        Property<T>(
            this IEnumerator<PSPropertyInfo> properties,
            string                           name,
            T                                value,
            Func<T, T, bool>?                comparison = null
        )
    {
        properties.MoveNext().ShouldBeTrue();

        var property = properties.Current;

        property                .ShouldNotBeNull();
        property.Name           .ShouldBe(name);
        property.TypeNameOfValue.ShouldBe(typeof(T).FullName);
        property.IsInstance     .ShouldBeTrue();
        property.IsGettable     .ShouldBe(true);
        property.IsSettable     .ShouldBe(true);

        if (comparison != null)
            comparison((T) property.Value, value).ShouldBeTrue();
        else
            property.Value.ShouldBe(value);

        return properties;
    }
}
