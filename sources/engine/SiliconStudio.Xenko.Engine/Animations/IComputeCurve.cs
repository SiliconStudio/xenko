// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// Base interface for curve based compute value nodes.
    /// </summary>
    [InlineProperty]
    public interface IComputeCurve<out T> where T : struct
    {
        /// <summary>
        /// Samples the compute curve at the specified location, usually in the [0 .. 1] range
        /// </summary>
        /// <param name="location">Location to sample at</param>
        /// <returns>Sampled value</returns>
        T SampleAt(float location);
    }
}
