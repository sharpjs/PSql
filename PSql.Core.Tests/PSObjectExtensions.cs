using System;
using System.Collections.Generic;
using System.Management.Automation;
using FluentAssertions;

namespace PSql
{
    internal static class PSObjectExtensions
    {
        internal static void
            ShouldHaveProperties(
                this PSObject                       obj,
                Action<IEnumerator<PSPropertyInfo>> assertion
            )
        {
            using var properties = obj.Properties.GetEnumerator();

            assertion(properties);

            properties.MoveNext().Should().BeFalse();
        }

        internal static IEnumerator<PSPropertyInfo>
            Property<T>(
                this IEnumerator<PSPropertyInfo> properties,
                string                           name,
                T                                value,
                bool                             gettable = true,
                bool                             settable = true
            )
        {
            properties.MoveNext().Should().BeTrue();

            var property = properties.Current;

            property                .Should().NotBeNull();
            property.Name           .Should().Be(name);
            property.TypeNameOfValue.Should().Be(typeof(T).FullName);
            property.IsInstance     .Should().BeTrue();
            property.IsGettable     .Should().Be(gettable);
            property.IsSettable     .Should().Be(settable);
            property.Value          .Should().Be(value);

            return properties;
        }
    }
}
