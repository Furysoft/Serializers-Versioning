// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestEntityThree.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Tests.TestEntities
{
    using System.Runtime.Serialization;
    using Furysoft.Versioning;

    /// <summary>
    /// The Test Entity One
    /// </summary>
    [DataContract]
    [DtoVersion(typeof(TestEntityThree), 1, 0, 0)]
    public sealed class TestEntityThree
    {
        /// <summary>
        /// Gets or sets the value1.
        /// </summary>
        [DataMember(Name = nameof(Value1), Order = 1)]
        public decimal Value1 { get; set; }
    }
}