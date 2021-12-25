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

using FluentAssertions;

namespace PSql.Tests;

using static ScriptExecutor;

internal static class StringExtensions
{
    internal static void
        ShouldOutput(
            this string     script,
            params object[] expected
        )
    {
        if (script is null)
            throw new ArgumentNullException(nameof(script));
        if (expected is null)
            throw new ArgumentNullException(nameof(expected));

        var (objects, exception) = Execute(script);

        exception.Should().BeNull();

        objects.Should().HaveCount(expected.Length);

        objects
            .Select(o => o?.BaseObject)
            .Should().BeEquivalentTo(expected, x => x.IgnoringCyclicReferences());
    }

    internal static void
        ShouldThrow<T>(this string script, string messagePart)
        where T : Exception
    {
        var exception = script.ShouldThrow<T>();

        exception.Message.Should().Contain(messagePart);
    }

    internal static T ShouldThrow<T>(this string script)
        where T : Exception
    {
        if (script is null)
            throw new ArgumentNullException(nameof(script));

        var (_, exception) = Execute(script);

        return exception
            .Should().NotBeNull()
            .And     .BeAssignableTo<T>()
            .Subject;
    }
}
