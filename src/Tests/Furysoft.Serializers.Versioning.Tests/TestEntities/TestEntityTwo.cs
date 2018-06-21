// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestEntityTwo.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Tests.TestEntities
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The Test Entity One
    /// </summary>
    [DataContract]
    public sealed class TestEntityTwo
    {
        /// <summary>
        /// Gets or sets the value1.
        /// </summary>
        [DataMember(Name = nameof(Value1), Order = 1)]
        public string Value1 { get; set; }

        /// <summary>
        /// Gets or sets the value2.
        /// </summary>
        [DataMember(Name = nameof(Value2), Order = 2)]
        public DateTime Value2 { get; set; }
    }
}