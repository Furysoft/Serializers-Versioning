// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionedMessageHandler.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Handlers
{
    using System;
    using Entities;
    using Furysoft.Versioning;
    using JetBrains.Annotations;

    /// <summary>
    /// The Versioned Message Handler
    /// </summary>
    public sealed class VersionedMessageHandler
    {
        /// <summary>
        /// The message
        /// </summary>
        private readonly VersionedMessage message;

        /// <summary>
        /// The was processed
        /// </summary>
        private bool wasProcessed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedMessageHandler" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public VersionedMessageHandler(VersionedMessage message)
        {
            this.message = message;
        }

        /// <summary>
        /// Action to take if no action is found
        /// </summary>
        /// <param name="action">The action.</param>
        public void Else([InstantHandle] Action action)
        {
            if (!this.wasProcessed)
            {
                action();
            }
        }

        /// <summary>
        /// Ons the specified action.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public VersionedMessageHandler On<TEntity>(
            [InstantHandle] Action<TEntity> action,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
            where TEntity : class
        {
            if (this.message.Version == typeof(TEntity).GetVersion())
            {
                var data = this.message.Data.Deserialize<TEntity>(serializerType);
                action(data);
                this.wasProcessed = true;
            }

            return this;
        }

        /// <summary>
        /// Ons the specified versioned message.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dtoVersion">The dto version.</param>
        /// <param name="action">The action.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public VersionedMessageHandler On<TEntity>(
            DtoVersion dtoVersion,
            [InstantHandle] Action<TEntity> action,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
            where TEntity : class
        {
            if (this.message.Version == dtoVersion)
            {
                var data = this.message.Data.Deserialize<TEntity>(serializerType);
                action(data);
                this.wasProcessed = true;
            }

            return this;
        }
    }
}