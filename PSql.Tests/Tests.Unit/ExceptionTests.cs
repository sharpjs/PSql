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

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using NUnit.Framework;
using static System.Reflection.BindingFlags;

namespace PSql.Tests.Unit;

public abstract class ExceptionTests<T>
    where T : Exception
{
    // As told by Dennis Ritchie:
    // https://www.bell-labs.com/usr/dmr/www/odd.html
    private const string ArcaneMessage = "values of β will give rise to dom!";

    [Test]
    public virtual void Construct_Default()
    {
        var exception = Create();

        exception.Message       .Should().NotBeNullOrWhiteSpace();
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public virtual void Construct_Message()
    {
        var exception = Create(ArcaneMessage);

        exception.Message       .Should().BeSameAs(ArcaneMessage);
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public virtual void Construct_Message_Null()
    {
        var exception = Create(null as string);

        exception.Message       .Should().NotBeNullOrWhiteSpace();
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public virtual void Construct_MessageAndInnerException()
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(ArcaneMessage, innerException);

        exception.Message       .Should().BeSameAs(ArcaneMessage);
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Test]
    public virtual void Construct_MessageAndInnerException_NullMessage()
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(null as string, innerException);

        exception.Message       .Should().NotBeNullOrWhiteSpace();
        exception.InnerException.Should().BeSameAs(innerException);
    }

    [Test]
    public virtual void Construct_MessageAndInnerException_NullInnerException()
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(ArcaneMessage, null as Exception);

        exception.Message       .Should().BeSameAs(ArcaneMessage);
        exception.InnerException.Should().BeNull();
    }

    [Test]
    public void SerializableAttribute()
    {
        typeof(T).IsSerializable.Should().BeTrue();
    }

    [Test]
    public void DeserializationConstructor()
    {
        var constructor = typeof(T).GetConstructor(
            Instance | Public | NonPublic | ExactBinding,
            null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) },
            null
        );

        constructor.Should().NotBeNull(
            "exception type {0} must provide a deserialization constructor; " +
            "see notes in the test for further information",
            typeof(T).FullName
        );

        constructor!.IsFamily.Should().BeTrue(
            "exception type {0} deserialization constructor must be protected; " +
            "see notes in the test for further information",
            typeof(T).FullName
        );
    }

    [Test]
    public virtual void SerializeThenDeserialize() // tests protected serialization constructor
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(ArcaneMessage, innerException);

        var deserialized   = Roundtrip(exception);

        deserialized               .Should().NotBeNull();
        deserialized.Message       .Should().Be(ArcaneMessage);
        deserialized.InnerException.Should().BeOfType<InvalidProgramException>();
    }

    private static T Create(params object?[] args)
    {
        // T is Exception or some type derived from it, not Nullable<_>.
        // Thus Activator.CreateInstance is guaranteed to not return null.
        return (T) Activator.CreateInstance(typeof(T), args)!;
    }

    private static T Roundtrip(T obj)
    {
        using (var memory = new MemoryStream())
        {
            // Serialize
            var formatter = new BinaryFormatter();
            formatter.Serialize(memory, obj);

            // Rewind
            memory.Position = 0;

            // Deserialize
            return (T) formatter.Deserialize(memory);
        }
    }
}
