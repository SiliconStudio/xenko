// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Physics
{
    public enum ConstraintTypes
    {
        /// <summary>
        ///     The translation vector of the matrix to create this will represent the pivot, the rest is ignored
        /// </summary>
        Point2Point,

        Hinge,

        Slider,

        ConeTwist,

        Generic6DoF,

        Generic6DoFSpring,

        /// <summary>
        ///     The translation vector of the matrix to create this will represent the axis, the rest is ignored
        /// </summary>
        Gear,
    }
}