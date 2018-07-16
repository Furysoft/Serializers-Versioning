// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncVersionedMessageHandler{TResponse}.cs" company="Simon Paramore">
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
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    [SuppressMessage("StyleCop", "SA1008", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    [SuppressMessage("StyleCop", "SA1009", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    public sealed class AsyncVersionedMessageHandler<TResponse>
    {
        /// <summary>
        /// The actions
        /// </summary>
        private readonly Dictionary<DtoVersion, (Type type, Func<object, Task<TResponse>> action)> actions = new Dictionary<DtoVersion, (Type type, Func<object, Task<TResponse>> action)>();

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
        private Func<string, Task<TResponse>> defaultAction;

        /// <summary>
        /// The on error
        /// </summary>
        private Func<Exception, Task<TResponse>> onError;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVersionedMessageHandler{TResponse}"/> class.
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
        public AsyncVersionedMessageHandler<TResponse> Else([InstantHandle] Func<string, Task<TResponse>> func)
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
        public AsyncVersionedMessageHandler<TResponse> On<TEntity>([InstantHandle] Func<TEntity, Task<TResponse>> func)
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
        public AsyncVersionedMessageHandler<TResponse> On<TEntity>([NotNull] DtoVersion dtoVersion, [InstantHandle] Func<TEntity, Task<TResponse>> func)
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
        public AsyncVersionedMessageHandler<TResponse> OnError([InstantHandle] Func<Exception, Task<TResponse>> func)
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
        public async Task<IEnumerable<TResponse>> PostAsync(BatchedVersionedMessage message)
        {
            var rtn = new List<TResponse>();
            foreach (var versionedMessage in message.Messages)
            {
                var response = await this.PostAsync(versionedMessage).ConfigureAwait(false);
                rtn.Add(response);
            }

            return rtn;
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task<TResponse> PostAsync(VersionedMessage message)
        {
            var thrown = default(Exception);
            var isProcessed = false;

            if (this.actions.TryGetValue(message.Version, out var actionType))
            {
                var deserialize = message.Data.Deserialize(actionType.type, this.serializerType);

                try
                {
                    var response = await actionType.action(deserialize).ConfigureAwait(false);
                    isProcessed = true;
                    return response;
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
                var defaultResponse = await this.defaultAction(message.Data).ConfigureAwait(false);
                return defaultResponse;
            }

            var errorResponse = await this.onError(thrown).ConfigureAwait(false);

            return errorResponse;
        }
    }
}