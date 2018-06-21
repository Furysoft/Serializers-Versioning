// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionedMessageSerializersTests.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Tests
{
    using System;
    using System.Diagnostics;
    using Entities;
    using Furysoft.Versioning;
    using NUnit.Framework;
    using TestEntities;

    /// <summary>
    /// The Versioned Message Serializer Tests
    /// </summary>
    [TestFixture]
    public sealed class VersionedMessageSerializersTests : TestBase
    {
        /// <summary>
        /// Serializes to versioned message when entity expect correct versioned message.
        /// </summary>
        [Test]
        public void SerializeToVersionedMessage_WhenEntity_ExpectCorrectVersionedMessage()
        {
            // Arrange
            var testEntityOne = new TestEntityOne { Value1 = "test1", Value2 = 42 };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var vm = testEntityOne.SerializeToVersionedMessage(SerializerType.Json);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(vm, Is.Not.Null);

            Assert.That(vm.Version, Is.EqualTo(new DtoVersion(typeof(TestEntityOne), 1, 0,0)));
            Assert.That(vm.Data, Is.EqualTo("{\"Value1\":\"test1\",\"Value2\":42}"));
        }

        /// <summary>
        /// Serializes to versioned message when entity with declared dto version expect correct versioned message.
        /// </summary>
        [Test]
        public void SerializeToVersionedMessage_WhenEntityWithDeclaredDtoVersion_ExpectCorrectVersionedMessage()
        {
            // Arrange
            var testEntityOne = new TestEntityTwo { Value1 = "test1", Value2 = new DateTime(2018,1,1) };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var vm = testEntityOne.SerializeToVersionedMessage(new DtoVersion(typeof(TestEntityTwo), 1, 0, 0), SerializerType.Json);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(vm, Is.Not.Null);

            Assert.That(vm.Version, Is.EqualTo(new DtoVersion(typeof(TestEntityTwo), 1, 0, 0)));
            Assert.That(vm.Data, Is.EqualTo("{\"Value1\":\"test1\",\"Value2\":\"2018-01-01T00:00:00\"}"));
        }

        /// <summary>
        /// Deserializes to versioned message when versioned message expect correct entity.
        /// </summary>
        [Test]
        public void DeserializeToVersionedMessage_WhenVersionedMessage_ExpectCorrectEntity()
        {
            // Arrange
            var testEntityOne = new TestEntityOne { Value1 = "test1", Value2 = 42 }
                .SerializeToVersionedMessage(SerializerType.Json).SerializeToString();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var vm = testEntityOne.DeserializeToVersionedMessage(SerializerType.ProtocolBuffers);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(vm, Is.Not.Null);

            Assert.That(vm.Version, Is.EqualTo(new DtoVersion(typeof(TestEntityOne), 1, 0, 0)));
            Assert.That(vm.Data, Is.EqualTo("{\"Value1\":\"test1\",\"Value2\":42}"));
        }

        /// <summary>
        /// Deserializes to versioned message when versioned message with custom version expect correct entity.
        /// </summary>
        [Test]
        public void DeserializeToVersionedMessage_WhenVersionedMessageWithCustomVersion_ExpectCorrectEntity()
        {
            // Arrange
            var testEntityOne = new TestEntityTwo { Value1 = "test1", Value2 = new DateTime(2018, 1, 1) }
                .SerializeToVersionedMessage(SerializerType.Json).SerializeToString();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var vm = testEntityOne.DeserializeToVersionedMessage(new DtoVersion(typeof(TestEntityTwo), 1, 0, 0), SerializerType.ProtocolBuffers);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(vm, Is.Not.Null);

            Assert.That(vm.Version, Is.EqualTo(new DtoVersion(typeof(TestEntityTwo), 1, 0, 0)));
            Assert.That(vm.Data, Is.EqualTo("{\"Value1\":\"test1\",\"Value2\":\"2018-01-01T00:00:00\"}"));
        }
    }
}