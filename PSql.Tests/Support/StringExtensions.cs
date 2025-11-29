// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

using static ScriptExecutor;

internal static class StringExtensions
{
    internal static void
        ShouldOutput(
            this string     script,
            params object[] expected
        )
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(expected);

        var (objects, exception) = Execute(script);

        exception.ShouldBeNull();

        objects.Count.ShouldBe(expected.Length);

        objects.Select(o => o?.BaseObject).ToArray()
            .ShouldBe(expected, StructuralEqualityComparer.Instance);
    }

    internal static void
        ShouldThrow<T>(this string script, string messagePart)
        where T : Exception
    {
        var exception = script.ShouldThrow<T>();

        exception.Message.ShouldContain(messagePart);
    }

    internal static T ShouldThrow<T>(this string script)
        where T : Exception
    {
        ArgumentNullException.ThrowIfNull(script);

        var (_, exception) = Execute(script);

        return exception
            .ShouldNotBeNull()
            .ShouldBeAssignableTo<T>();
    }
}
