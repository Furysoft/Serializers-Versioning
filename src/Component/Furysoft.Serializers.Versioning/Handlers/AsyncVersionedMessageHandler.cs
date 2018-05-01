// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncVersionedMessageHandler.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Entities;
    using Furysoft.Versioning;
    using JetBrains.Annotations;

    /// <summary>
    /// The Async Versioned Message Handler
    /// </summary>
    public sealed class AsyncVersionedMessageHandler
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
        /// Initializes a new instance of the <see cref="AsyncVersionedMessageHandler" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public AsyncVersionedMessageHandler(VersionedMessage message)
        {
            this.message = message;

            this.Task = System.Threading.Tasks.Task.FromResult(System.Threading.Tasks.Task.CompletedTask);
        }

        /// <summary>
        /// Gets the task.
        /// </summary>
        public Task<Task> Task { get; private set; }

        /// <summary>
        /// Else the specified function.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Else([InstantHandle] Action<string> func)
        {
            if (!this.wasProcessed)
            {
                func(this.message.Data);
            }

            await this.Task.Unwrap().ConfigureAwait(false);
        }

        /// <summary>
        /// Action to take if no action is found
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns>
        /// The <see cref="Task" />
        /// </returns>
        public async Task ElseAsync([InstantHandle] Func<string, Task> func)
        {
            if (!this.wasProcessed)
            {
                this.Task = this.Task.ContinueWith(
                    t => func(this.message.Data), TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            await this.Task.Unwrap().ConfigureAwait(false);
        }

        /// <summary>
        /// Called when [asynchronous].
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>
        /// The <see cref="AsyncVersionedMessageHandler" />
        /// </returns>
        public AsyncVersionedMessageHandler OnAsync<TEntity>(
            [InstantHandle] Func<TEntity, Task> func,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
            where TEntity : class
        {
            if (this.message.Version == typeof(TEntity).GetVersion())
            {
                var data = this.message.Data.Deserialize<TEntity>(serializerType);

                this.Task = this.Task.ContinueWith(
                    t => func(data), TaskContinuationOptions.OnlyOnRanToCompletion);

                this.wasProcessed = true;
            }

            return this;
        }

        /// <summary>
        /// Ons the specified dto version.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dtoVersion">The dto version.</param>
        /// <param name="func">The function.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>
        /// The <see cref="AsyncVersionedMessageHandler" />
        /// </returns>
        public async Task<AsyncVersionedMessageHandler> OnAsync<TEntity>(
            DtoVersion dtoVersion,
            [InstantHandle] Func<TEntity, Task> func,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
            where TEntity : class
        {
            if (this.message.Version == dtoVersion)
            {
                var data = this.message.Data.Deserialize<TEntity>(serializerType);
                await func(data).ConfigureAwait(false);
                this.wasProcessed = true;
            }

            return this;
        }
    }
}