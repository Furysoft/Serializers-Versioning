// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionedMessageHandler.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Furysoft.Serializers.Entities;
    using Furysoft.Versioning;
    using JetBrains.Annotations;

    /// <summary>
    /// The Versioned Message Handler.
    /// </summary>
    [SuppressMessage("StyleCop", "SA1008", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    [SuppressMessage("StyleCop", "SA1009", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    public sealed class VersionedMessageHandler
    {
        /// <summary>
        /// The actions.
        /// </summary>
        private readonly Dictionary<DtoVersion, (Type type, Action<object> action)> actions = new Dictionary<DtoVersion, (Type type, Action<object> action)>();

        /// <summary>
        /// The serializer type.
        /// </summary>
        private readonly SerializerType localSerializerType;

        /// <summary>
        /// The throw on error.
        /// </summary>
        private readonly bool throwOnError;

        /// <summary>
        /// The default action.
        /// </summary>
        private Action<string> defaultAction;

        /// <summary>
        /// The on error.
        /// </summary>
        private Action<Exception> onError;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedMessageHandler" /> class.
        /// </summary>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        public VersionedMessageHandler(SerializerType serializerType, bool throwOnError)
        {
            this.localSerializerType = serializerType;
            this.throwOnError = throwOnError;
        }

        /// <summary>
        /// Action to take if no action is found.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="VersionedMessageHandler"/>.</returns>
        public VersionedMessageHandler Else([InstantHandle] Action<string> action)
        {
            this.defaultAction = action;

            return this;
        }

        /// <summary>
        /// Ons the specified action.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />.
        /// </returns>
        public VersionedMessageHandler On<TEntity>([InstantHandle] Action<TEntity> action)
            where TEntity : class
        {
            var type = typeof(TEntity);

            this.actions.Add(type.GetVersion(), (type, o => action((TEntity)o)));

            return this;
        }

        /// <summary>
        /// Ons the specified action.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dtoVersion">The dto version.</param>
        /// <param name="action">The action.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />.
        /// </returns>
        public VersionedMessageHandler On<TEntity>([NotNull] DtoVersion dtoVersion, [InstantHandle] Action<TEntity> action)
            where TEntity : class
        {
            var type = typeof(TEntity);

            this.actions.Add(dtoVersion, (type, o => action((TEntity)o)));

            return this;
        }

        /// <summary>
        /// Called when [error].
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="VersionedMessageHandler"/>.</returns>
        public VersionedMessageHandler OnError([InstantHandle] Action<Exception> action)
        {
            this.onError = action;

            return this;
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        public void Post(VersionedMessage message, SerializerType serializerType = SerializerType.None)
        {
            var thrown = default(Exception);
            var isProcessed = false;
            var serializer = serializerType == SerializerType.None ? this.localSerializerType : serializerType;

            if (this.actions.TryGetValue(message.Version, out var actionType))
            {
                var deserialize = message.Data.Deserialize(actionType.type, serializer);

                try
                {
                    actionType.action(deserialize);
                    isProcessed = true;
                    return;
                }
                catch (Exception e)
                {
                    thrown = e;
                    if (this.throwOnError)
                    {
                        throw;
                    }
                }
            }

            if (!isProcessed && thrown == null)
            {
                this.defaultAction?.Invoke(message.Data);

                return;
            }

            this.onError?.Invoke(thrown);
        }

        /// <summary>
        /// Posts the specified message, handling without deserializing the message itself.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="action">The action.</param>
        public void Post(VersionedMessage message, Action<VersionedMessage> action)
        {
            Exception thrown;

            try
            {
                action(message);
                return;
            }
            catch (Exception e)
            {
                thrown = e;
                if (this.throwOnError)
                {
                    throw;
                }
            }

            this.onError?.Invoke(thrown);
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        public void Post(BatchedVersionedMessage message, SerializerType serializerType = SerializerType.None)
        {
            foreach (var versionedMessage in message.Messages)
            {
                this.Post(versionedMessage, serializerType);
            }
        }
    }
}