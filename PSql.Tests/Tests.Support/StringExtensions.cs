using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;

namespace PSql.Tests
{
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

            objects.Select(o => o?.BaseObject).Should().BeEquivalentTo(expected);
        }

        internal static void
            ShouldThrow<T>(
                this string script,
                string      messagePart
            )
            where T : Exception
        {
            script.ShouldThrow<T>(
                e => e.Message.Contains(messagePart, StringComparison.OrdinalIgnoreCase)
            );
        }

        internal static void
            ShouldThrow<T>(
                this string                script,
                Expression<Func<T, bool>>? predicate = null
            )
            where T : Exception
        {
            if (script is null)
                throw new ArgumentNullException(nameof(script));

            var (_, exception) = Execute(script);

            exception.Should().NotBeNull().And.BeAssignableTo<T>();

            if (predicate is not null)
                exception.Should().Match<T>(predicate);
        }
    }
}
