// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// Base interface for curve based compute value nodes.
    /// </summary>
    public class ComputeCurveSampler<T> where T : struct
    {
        [NotNull]
        [Display("Curve")]
        public IComputeCurve<T> Curve { get; set; } = new ComputeConstCurve<T>();

        /// <summary>
        /// Samples the compute curve at the specified location, usually in the [0 .. 1] range
        /// </summary>
        /// <param name="location">Location to sample at</param>
        /// <returns>Sampled value</returns>
        T SampleAt(float t)
        {
            return Curve?.SampleAt(t) ?? new T();
        }
    }
}
