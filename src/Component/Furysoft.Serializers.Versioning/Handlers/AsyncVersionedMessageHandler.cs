// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncVersionedMessageHandler.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Entities;
    using Furysoft.Versioning;
    using JetBrains.Annotations;

    /// <summary>
    /// The Async Versioned Message Handler
    /// </summary>
    [SuppressMessage("StyleCop", "SA1008", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    [SuppressMessage("StyleCop", "SA1009", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    public sealed class AsyncVersionedMessageHandler
    {
        /// <summary>
        /// The actions
        /// </summary>
        private readonly Dictionary<DtoVersion, (Type type, Func<object, Task> action)> actions = new Dictionary<DtoVersion, (Type type, Func<object, Task> action)>();

        /// <summary>
        /// The serializer type
        /// </summary>
        private readonly SerializerType serializerType;

        /// <summary>
        /// The throw on error
        /// </summary>
        private readonly bool throwOnError;

        /// <summary>
        /// The default action
        /// </summary>
        private Func<string, Task> defaultAction;

        /// <summary>
        /// The on error
        /// </summary>
        private Func<Exception, Task> onError;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVersionedMessageHandler"/> class.
        /// </summary>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        public AsyncVersionedMessageHandler(SerializerType serializerType, bool throwOnError)
        {
            this.serializerType = serializerType;
            this.throwOnError = throwOnError;
        }

        /// <summary>
        /// Action to take if no action is found
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public AsyncVersionedMessageHandler Else([InstantHandle] Func<string, Task> func)
        {
            this.defaultAction = func;

            return this;
        }

        /// <summary>
        /// Ons the specified action.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public AsyncVersionedMessageHandler On<TEntity>([InstantHandle] Func<TEntity, Task> func)
            where TEntity : class
        {
            var type = typeof(TEntity);

            this.actions.Add(type.GetVersion(), (type, o => func((TEntity)o)));

            return this;
        }

        /// <summary>
        /// Ons the specified action.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dtoVersion">The dto version.</param>
        /// <param name="func">The function.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public AsyncVersionedMessageHandler On<TEntity>([NotNull] DtoVersion dtoVersion, [InstantHandle] Func<TEntity, Task> func)
            where TEntity : class
        {
            var type = typeof(TEntity);

            this.actions.Add(dtoVersion, (type, o => func((TEntity)o)));

            return this;
        }

        /// <summary>
        /// Called when [error].
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns>
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public AsyncVersionedMessageHandler OnError([InstantHandle] Func<Exception, Task> func)
        {
            this.onError = func;

            return this;
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// The <see cref="Task" />
        /// </returns>
        public async Task PostAsync(BatchedVersionedMessage message)
        {
            foreach (var versionedMessage in message.Messages)
            {
                await this.PostAsync(versionedMessage).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PostAsync(VersionedMessage message)
        {
            var thrown = default(Exception);
            var isProcessed = false;

            if (this.actions.TryGetValue(message.Version, out var actionType))
            {
                var deserialize = message.Data.Deserialize(actionType.type, this.serializerType);

                try
                {
                    await actionType.action(deserialize).ConfigureAwait(false);
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
                if (this.defaultAction != null)
                {
                    await this.defaultAction(message.Data).ConfigureAwait(false);
                }

                return;
            }

            if (this.onError != null)
            {
                await this.onError(thrown).ConfigureAwait(false);
            }
        }
    }
}