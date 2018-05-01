// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Simon Paramore">
// © 2017, Simon Paramore
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Furysoft.Serializers.Versioning.Tests
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// The Test Base
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Writes the time elapsed.
        /// </summary>
        /// <param name="sw">The sw.</param>
        protected void WriteTimeElapsed(Stopwatch sw)
        {
            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms ({sw.Elapsed})");
        }
    }
}