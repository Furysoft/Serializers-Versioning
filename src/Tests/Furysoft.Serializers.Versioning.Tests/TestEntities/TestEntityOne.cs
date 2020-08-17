// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestEntityOne.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Tests.TestEntities
{
    using System.Runtime.Serialization;
    using Furysoft.Versioning;

    /// <summary>
    /// The Test Entity One.
    /// </summary>
    [DataContract]
    [DtoVersion(typeof(TestEntityOne), 1, 0, 0)]
    public sealed class TestEntityOne
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
        public int Value2 { get; set; }
    }
}