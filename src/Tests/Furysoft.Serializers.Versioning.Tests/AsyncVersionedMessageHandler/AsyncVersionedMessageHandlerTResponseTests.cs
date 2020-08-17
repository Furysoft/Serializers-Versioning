// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncVersionedMessageHandlerTResponseTests.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Tests.AsyncVersionedMessageHandler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Furysoft.Serializers.Entities;
    using Furysoft.Serializers.Versioning.Handlers;
    using Furysoft.Serializers.Versioning.Tests.TestEntities;
    using Furysoft.Versioning;
    using NUnit.Framework;

    /// <summary>
    /// The AsyncVersionedMessageHandler Tests.
    /// </summary>
    [TestFixture]
    public sealed class AsyncVersionedMessageHandlerTResponseTests : TestBase
    {
        /// <summary>
        /// Versioned the message handler when batched versioned message expect all processed.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Test]
        public async Task VersionedMessageHandler_WhenBatchedVersionedMessage_ExpectAllProcessed()
        {
            // Arrange
            var exception = default(Exception);

            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityOne>(SerializerType.ProtocolBuffers, true)
                .On<TestEntityOne>(
                    e => Task.FromResult(new TestEntityOne
                    {
                        Value1 = e.Value1,
                        Value2 = e.Value2,
                    }))
                .On<TestEntityTwo>(
                    e => Task.FromResult(new TestEntityOne
                    {
                        Value1 = e.Value1,
                        Value2 = 5,
                    }))
                .Else(
                    s => Task.FromResult(new TestEntityOne
                    {
                        Value1 = "TestVal",
                        Value2 = 5,
                    }))
                .OnError(
                    e =>
                    {
                        exception = e;
                        return Task.FromResult(default(TestEntityOne));
                    });

            var batchedVersionedMessage = new BatchedVersionedMessage
            {
                Messages = new List<VersionedMessage>
                {
                    new TestEntityOne { Value1 = "test", Value2 = 42 }.SerializeToVersionedMessage(),
                    new TestEntityTwo { Value1 = "Value1", Value2 = new DateTime(2018, 1, 1) }.SerializeToVersionedMessage(),
                    new TestEntityThree { Value1 = 3 }.SerializeToVersionedMessage(),
                },
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var results = await versionedMessageHandler.PostAsync(batchedVersionedMessage).ConfigureAwait(false);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            var resultsList = results.ToList();

            Assert.That(resultsList[0], Is.Not.Null);
            Assert.That(resultsList[1], Is.Not.Null);
            Assert.That(resultsList[2], Is.Not.Null);
            Assert.That(exception, Is.Null);

            Assert.That(resultsList[0].Value1, Is.EqualTo("test"));
            Assert.That(resultsList[0].Value2, Is.EqualTo(42));

            Assert.That(resultsList[1].Value1, Is.EqualTo("Value1"));
            Assert.That(resultsList[1].Value2, Is.EqualTo(5));

            Assert.That(resultsList[2].Value1, Is.EqualTo("TestVal"));
            Assert.That(resultsList[2].Value2, Is.EqualTo(5));
        }

        /// <summary>
        /// Versioned the message handler when different serializations expect all processed.
        /// </summary>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        [Test]
        public async Task VersionedMessageHandler_WhenDifferentSerializations_ExpectAllProcessed()
        {
            // Arrange
            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityOne>(SerializerType.ProtocolBuffers, true)
                .On<TestEntityOne>(Task.FromResult);

            var e1 = new TestEntityOne { Value1 = "test1", Value2 = 42 }.SerializeToVersionedMessage();
            var e2 = new TestEntityOne { Value1 = "test2", Value2 = 42 }.SerializeToVersionedMessage();
            var e3 = new TestEntityOne { Value1 = "test3", Value2 = 42 }.SerializeToVersionedMessage(SerializerType.Json);
            var e4 = new TestEntityOne { Value1 = "test4", Value2 = 42 }.SerializeToVersionedMessage(SerializerType.Xml);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var r1 = await versionedMessageHandler.PostAsync(e1).ConfigureAwait(false);
            var r2 = await versionedMessageHandler.PostAsync(e2, SerializerType.ProtocolBuffers).ConfigureAwait(false);
            var r3 = await versionedMessageHandler.PostAsync(e3, SerializerType.Json).ConfigureAwait(false);
            var r4 = await versionedMessageHandler.PostAsync(e4, SerializerType.Xml).ConfigureAwait(false);

            var responses = new List<TestEntityOne>
            {
                r1,
                r2,
                r3,
                r4,
            };

            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(responses.Count, Is.EqualTo(4));

            Assert.That(responses[0].Value1, Is.EqualTo("test1"));
            Assert.That(responses[1].Value1, Is.EqualTo("test2"));
            Assert.That(responses[2].Value1, Is.EqualTo("test3"));
            Assert.That(responses[3].Value1, Is.EqualTo("test4"));
        }

        /// <summary>
        /// Versioned the message handler when error and not throw on error expect handled.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Test]
        public async Task VersionedMessageHandler_WhenErrorAndNotThrowOnError_ExpectHandled()
        {
            // Arrange
            var defaultValue = default(string);
            var exception = default(Exception);

            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityOne>(SerializerType.Json, false)
                .On<TestEntityOne>(e => throw new DivideByZeroException())
                .On<TestEntityTwo>(
                    e => Task.FromResult(
                        new TestEntityOne
                        {
                            Value2 = 5,
                            Value1 = e.Value1,
                        }))
                .Else(
                    s =>
                    {
                        defaultValue = s;
                        return Task.FromResult(default(TestEntityOne));
                    })
                .OnError(
                    e =>
                    {
                        exception = e;
                        return Task.FromResult(default(TestEntityOne));
                    });

            var versionedMessage = new VersionedMessage
            {
                Data = new TestEntityOne().SerializeToString(SerializerType.Json),
                Version = typeof(TestEntityOne).GetVersion(),
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var rtn = await versionedMessageHandler.PostAsync(versionedMessage).ConfigureAwait(false);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(rtn, Is.Null);
            Assert.That(defaultValue, Is.Null);
            Assert.That(exception, Is.Not.Null);

            Assert.That(exception.GetType(), Is.EqualTo(typeof(DivideByZeroException)));
        }

        /// <summary>
        /// Versioned the message handler when error and throw on error expect throws.
        /// </summary>
        [Test]
        public void VersionedMessageHandler_WhenErrorAndThrowOnError_ExpectThrows()
        {
            // Arrange
            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityOne>(SerializerType.Json, true)
                .On<TestEntityOne>(e => throw new DivideByZeroException())
                .On<TestEntityTwo>(e => Task.FromResult(default(TestEntityOne)))
                .Else(s => Task.FromResult(default(TestEntityOne)))
                .OnError(e => Task.FromResult(default(TestEntityOne)));

            var versionedMessage = new VersionedMessage
            {
                Data = new TestEntityOne().SerializeToString(SerializerType.Json),
                Version = typeof(TestEntityOne).GetVersion(),
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            Assert.ThrowsAsync<DivideByZeroException>(() => versionedMessageHandler.PostAsync(versionedMessage));
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);
        }

        /// <summary>
        /// Versioned the message handler when no match expect default.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Test]
        public async Task VersionedMessageHandler_WhenNoMatch_ExpectDefault()
        {
            // Arrange
            var exception = default(Exception);

            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityOne>(SerializerType.Json, true)
                .On<TestEntityOne>(e => Task.FromResult(default(TestEntityOne)))
                .On<TestEntityTwo>(e => Task.FromResult(default(TestEntityOne)))
                .Else(
                    s => Task.FromResult(new TestEntityOne
                    {
                        Value2 = 42,
                        Value1 = "DEFAULT",
                    }))
                .OnError(
                    e =>
                    {
                        exception = e;
                        return Task.FromResult(default(TestEntityOne));
                    });

            var versionedMessage = new VersionedMessage
            {
                Data = new TestEntityThree { Value1 = 25.3m }.SerializeToString(SerializerType.Json),
                Version = typeof(TestEntityThree).GetVersion(),
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var rtn = await versionedMessageHandler.PostAsync(versionedMessage).ConfigureAwait(false);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(exception, Is.Null);

            Assert.That(rtn.Value1, Is.EqualTo("DEFAULT"));
            Assert.That(rtn.Value2, Is.EqualTo(42));
        }

        /// <summary>
        /// Versioned message handler when version match expect action.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Test]
        public async Task VersionedMessageHandler_WhenVersionMatchOnFirst_ExpectAction()
        {
            // Arrange
            var exception = default(Exception);

            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityOne>(SerializerType.ProtocolBuffers, true)
                .On<TestEntityOne>(Task.FromResult)
                .On<TestEntityTwo>(e => Task.FromResult(default(TestEntityOne)))
                .Else(e => Task.FromResult(default(TestEntityOne)))
                .OnError(
                    e =>
                    {
                        exception = e;
                        return Task.FromResult(default(TestEntityOne));
                    });

            var versionedMessage = new VersionedMessage
            {
                Data = new TestEntityOne { Value1 = "test", Value2 = 42 }.SerializeToString(),
                Version = typeof(TestEntityOne).GetVersion(),
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var testEntityOne = await versionedMessageHandler.PostAsync(versionedMessage).ConfigureAwait(false);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(exception, Is.Null);

            Assert.That(testEntityOne.Value1, Is.EqualTo("test"));
            Assert.That(testEntityOne.Value2, Is.EqualTo(42));
        }

        /// <summary>
        /// Versioned message handler when version match expect action.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Test]
        public async Task VersionedMessageHandler_WhenVersionMatchOnSecond_ExpectAction()
        {
            // Arrange
            var exception = default(Exception);

            var versionedMessageHandler = new AsyncVersionedMessageHandler<TestEntityTwo>(SerializerType.ProtocolBuffers, true)
                .On<TestEntityOne>(e => Task.FromResult(default(TestEntityTwo)))
                .On<TestEntityTwo>(Task.FromResult)
                .Else(e => Task.FromResult(default(TestEntityTwo)))
                .OnError(
                    e =>
                    {
                        exception = e;
                        return Task.FromResult(default(TestEntityTwo));
                    });

            var versionedMessage = new VersionedMessage
            {
                Data = new TestEntityTwo { Value1 = "test", Value2 = new DateTime(2018, 1, 1) }.SerializeToString(),
                Version = typeof(TestEntityTwo).GetVersion(),
            };

            // Act
            var stopwatch = Stopwatch.StartNew();
            var testEntityTwo = await versionedMessageHandler.PostAsync(versionedMessage).ConfigureAwait(false);
            stopwatch.Stop();

            // Assert
            this.WriteTimeElapsed(stopwatch);

            Assert.That(exception, Is.Null);

            Assert.That(testEntityTwo.Value1, Is.EqualTo("test"));
            Assert.That(testEntityTwo.Value2, Is.EqualTo(new DateTime(2018, 1, 1)));
        }
    }
}