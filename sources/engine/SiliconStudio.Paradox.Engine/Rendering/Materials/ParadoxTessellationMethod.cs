// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Describe the different tessellation methods used in Paradox.
    /// </summary>
    [Flags]
    public enum ParadoxTessellationMethod
    {
        /// <summary>
        /// No tessellation
        /// </summary>
        None = 0,

        /// <summary>
        /// Flat tessellation. Also known as dicing tessellation.
        /// </summary>
        Flat = 1,

        /// <summary>
        /// Point normal tessellation.
        /// </summary>
        PointNormal = 1,

        /// <summary>
        /// Adjacent edge average.
        /// </summary>
        AdjacentEdgeAverage = 2,
    }

    public static class ParadoxTessellationMethodExtensions
    {
        public static bool PerformsAdjacentEdgeAverage(this ParadoxTessellationMethod method)
        {
            return (method & ParadoxTessellationMethod.AdjacentEdgeAverage) != 0;
        }

        public static PrimitiveType GetPrimitiveType(this ParadoxTessellationMethod method)
        {
            if((method & ParadoxTessellationMethod.PointNormal) == 0)
                return PrimitiveType.TriangleList;

            var controlsCount = method.PerformsAdjacentEdgeAverage() ? 12 : 3;
            return PrimitiveType.PatchList.ControlPointCount(controlsCount);
        }
    }
}