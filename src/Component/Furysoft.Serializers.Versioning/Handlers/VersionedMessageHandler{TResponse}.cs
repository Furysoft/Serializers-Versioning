// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionedMessageHandler{TResponse}.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Entities;
    using Furysoft.Versioning;
    using JetBrains.Annotations;

    /// <summary>
    /// The Versioned Message Handler
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    [SuppressMessage("StyleCop", "SA1008", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    [SuppressMessage("StyleCop", "SA1009", Justification = "StyleCop doesn't understand C#7 tuple return types yet.")]
    public sealed class VersionedMessageHandler<TResponse>
    {
        /// <summary>
        /// The actions
        /// </summary>
        private readonly Dictionary<DtoVersion, (Type type, Func<object, TResponse> action)> actions = new Dictionary<DtoVersion, (Type type, Func<object, TResponse> action)>();

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
        private Func<string, TResponse> defaultAction;

        /// <summary>
        /// The on error
        /// </summary>
        private Func<Exception, TResponse> onError;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedMessageHandler{TResponse}" /> class.
        /// </summary>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        public VersionedMessageHandler(SerializerType serializerType, bool throwOnError)
        {
            this.serializerType = serializerType;
            this.throwOnError = throwOnError;
        }

        /// <summary>
        /// Action to take if no action is found
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The <see cref="VersionedMessageHandler"/></returns>
        public VersionedMessageHandler<TResponse> Else([InstantHandle] Func<string, TResponse> action)
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
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public VersionedMessageHandler<TResponse> On<TEntity>([InstantHandle] Func<TEntity, TResponse> action)
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
        /// The <see cref="VersionedMessageHandler" />
        /// </returns>
        public VersionedMessageHandler<TResponse> On<TEntity>([NotNull] DtoVersion dtoVersion, [InstantHandle] Func<TEntity, TResponse> action)
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
        /// <returns>The <see cref="VersionedMessageHandler"/></returns>
        public VersionedMessageHandler<TResponse> OnError([InstantHandle] Func<Exception, TResponse> action)
        {
            this.onError = action;

            return this;
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The <see cref="!:TResponse"/></returns>
        public TResponse Post(VersionedMessage message)
        {
            var thrown = default(Exception);
            var isProcessed = false;

            if (this.actions.TryGetValue(message.Version, out var actionType))
            {
                var deserialize = message.Data.Deserialize(actionType.type, this.serializerType);

                try
                {
                    var rtn = actionType.action(deserialize);
                    isProcessed = true;
                    return rtn;
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
                var rtn = default(TResponse);
                if (this.defaultAction != null)
                {
                    rtn = this.defaultAction.Invoke(message.Data);
                }

                return rtn;
            }

            return this.onError != null
                ? this.onError(thrown)
                : default(TResponse);
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The list of <see cref="!:TResponse"/></returns>
        public IEnumerable<TResponse> Post(BatchedVersionedMessage message)
        {
            return message.Messages.Select(this.Post).ToList();
        }
    }
}