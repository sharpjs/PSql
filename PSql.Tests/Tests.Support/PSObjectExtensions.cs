/*
    Copyright 2021 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Management.Automation;
using FluentAssertions;

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
