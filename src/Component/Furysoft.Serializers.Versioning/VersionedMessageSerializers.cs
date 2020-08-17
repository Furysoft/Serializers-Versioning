// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionedMessageSerializers.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning
{
    using Furysoft.Serializers.Entities;
    using Furysoft.Versioning;

    /// <summary>
    /// The Versioned Message Serializers.
    /// </summary>
    public static class VersionedMessageSerializers
    {
        /// <summary>
        /// Deserializes to versioned message.
        /// </summary>
        /// <param name="serialized">The serialized.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>The <see cref="VersionedMessage"/>.</returns>
        public static VersionedMessage DeserializeToVersionedMessage(
            this string serialized,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
        {
            return serialized.Deserialize<VersionedMessage>(serializerType);
        }

        /// <summary>
        /// Deserializes to versioned message.
        /// </summary>
        /// <param name="serialized">The serialized.</param>
        /// <param name="dtoVersion">The dto version.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>The <see cref="VersionedMessage"/>.</returns>
        public static VersionedMessage DeserializeToVersionedMessage(
            this string serialized,
            DtoVersion dtoVersion,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
        {
            var rtn = serialized.Deserialize<VersionedMessage>(serializerType);
            rtn.Version = dtoVersion;
            return rtn;
        }

        /// <summary>
        /// Serializes to versioned message.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>The <see cref="VersionedMessage"/>.</returns>
        public static VersionedMessage SerializeToVersionedMessage<TEntity>(
            this TEntity entity,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
            where TEntity : class
        {
            var data = entity.SerializeToString(serializerType);
            var version = typeof(TEntity).GetVersion();

            return new VersionedMessage
            {
                Data = data,
                Version = version,
            };
        }

        /// <summary>
        /// Serializes to versioned message.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="dtoVersion">The dto version.</param>
        /// <param name="serializerType">Type of the serializer.</param>
        /// <returns>The <see cref="VersionedMessage"/>.</returns>
        public static VersionedMessage SerializeToVersionedMessage<TEntity>(
            this TEntity entity,
            DtoVersion dtoVersion,
            SerializerType serializerType = SerializerType.ProtocolBuffers)
            where TEntity : class
        {
            var data = entity.SerializeToString(serializerType);

            return new VersionedMessage
            {
                Data = data,
                Version = dtoVersion,
            };
        }
    }
}