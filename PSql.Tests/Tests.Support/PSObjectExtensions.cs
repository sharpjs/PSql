// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Management.Automation;

namespace PSql.Tests;

internal static class PSObjectExtensions
{
    internal static void
        ShouldHaveProperties(
            this PSObject?                      obj,
            Action<IEnumerator<PSPropertyInfo>> assertion
        )
    {
        obj.Should().NotBeNull();

        using var properties = obj!.Properties.GetEnumerator();

        assertion(properties);

        properties.MoveNext().Should().BeFalse();
    }

    internal static IEnumerator<PSPropertyInfo>
        Property<T>(
            this IEnumerator<PSPropertyInfo> properties,
            string                           name,
            T                                value,
            Func<T, T, bool>?                comparison = null
        )
    {
        properties.MoveNext().Should().BeTrue();

        var property = properties.Current;

        property                .Should().NotBeNull();
        property.Name           .Should().Be(name);
        property.TypeNameOfValue.Should().Be(typeof(T).FullName);
        property.IsInstance     .Should().BeTrue();
        property.IsGettable     .Should().Be(true);
        property.IsSettable     .Should().Be(true);

        if (comparison != null)
            comparison((T) property.Value, value).Should().BeTrue();
        else
            property.Value.Should().Be(value);

        return properties;
    }
}
