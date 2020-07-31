using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using NUnit.Framework;
using static System.Reflection.BindingFlags;

namespace PSql
{
    public abstract class ExceptionTests<T>
        where T : Exception
    {
        // As told by Dennis Ritchie:
        // https://www.bell-labs.com/usr/dmr/www/odd.html
        private const string ArcaneMessage = "values of β will give rise to dom!";

        [Test]
        public virtual void Construct_Default()
        {
            var exception = (T) Activator.CreateInstance(typeof(T));

            exception.Message       .Should().NotBeNullOrWhiteSpace();
            exception.InnerException.Should().BeNull();
        }

        [Test]
        public virtual void Construct_Message()
        {
            var exception = (T) Activator.CreateInstance(typeof(T), ArcaneMessage);

            exception.Message       .Should().BeSameAs(ArcaneMessage);
            exception.InnerException.Should().BeNull();
        }

        [Test]
        public virtual void Construct_Message_Null()
        {
            var exception = (T) Activator.CreateInstance(typeof(T), null as string);

            exception.Message       .Should().NotBeNullOrWhiteSpace();
            exception.InnerException.Should().BeNull();
        }

        [Test]
        public virtual void Construct_MessageAndInnerException()
        {
            var innerException = new InvalidProgramException();
            var exception      = (T) Activator.CreateInstance(typeof(T), ArcaneMessage, innerException);

            exception.Message       .Should().BeSameAs(ArcaneMessage);
            exception.InnerException.Should().BeSameAs(innerException);
        }

        [Test]
        public virtual void Construct_MessageAndInnerException_NullMessage()
        {
            var innerException = new InvalidProgramException();
            var exception      = (T) Activator.CreateInstance(typeof(T), null as string, innerException);

            exception.Message       .Should().NotBeNullOrWhiteSpace();
            exception.InnerException.Should().BeSameAs(innerException);
        }

        [Test]
        public virtual void Construct_MessageAndInnerException_NullInnerException()
        {
            var innerException = new InvalidProgramException();
            var exception      = (T) Activator.CreateInstance(typeof(T), ArcaneMessage, null as Exception);

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

            constructor.IsFamily.Should().BeTrue(
                "exception type {0} deserialization constructor must be protected; " +
                "see notes in the test for further information",
                typeof(T).FullName
            );
        }

        [Test]
        public virtual void SerializeThenDeserialize() // tests protected serialization constructor
        {
            var innerException = new InvalidProgramException();
            var exception      = (T) Activator.CreateInstance(typeof(T), ArcaneMessage, innerException);

            var deserialized   = Roundtrip(exception);

            deserialized               .Should().NotBeNull();
            deserialized.Message       .Should().Be(ArcaneMessage);
            deserialized.InnerException.Should().BeOfType<InvalidProgramException>();
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
}
