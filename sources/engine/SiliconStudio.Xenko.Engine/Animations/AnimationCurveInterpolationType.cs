// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Animations
{
    /// <summary>
    /// Describes how a curve should be interpolated.
    /// </summary>
    [DataContract]
    public enum AnimationCurveInterpolationType
    {
        /// <summary>
        /// Interpolates by using constant value between keyframes.
        /// </summary>
        Constant,

        /// <summary>
        /// Interpolates linearly between keyframes.
        /// </summary>
        Linear,

        /// <summary>
        /// Interpolates with implicit derivatives using points before and after.
        /// More information at http://en.wikipedia.org/wiki/Cubic_Hermite_spline#Interpolation_on_the_unit_interval_without_exact_derivatives.
        /// </summary>
        Cubic,
    }
}