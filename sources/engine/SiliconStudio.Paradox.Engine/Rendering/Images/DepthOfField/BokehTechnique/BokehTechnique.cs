// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Rendering.Images
{

    /// <summary>
    /// Techniques available to perform a DoF effect on a level.
    /// The technique directly affects the visual result (bokeh shape) as well as the performance. 
    /// </summary>
    public enum BokehTechnique
    {
        /// <summary>
        /// Circular blur using a gaussian. 
        /// </summary>
        /// <remarks>
        /// Fast and cheap technique but the final bokeh shapes are not very realistic.
        /// </remarks>
        CircularGaussian,

        /// <summary>
        /// Hexagonal blur using the McIntosh technique.
        /// </summary>
        HexagonalMcIntosh,

        /// <summary>
        /// Hexagonal blur using a combination of 3 rhombi blurs. 
        /// </summary>
        HexagonalTripleRhombi
    }


    // Extension methods to directly instantiate a blur image effect from a bokeh technique name.
    public static class BokehTechniqueExtensions
    {
        /// <summary>
        /// Instantiates a new <see cref="BokehBlur"/> from a technique name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A Bokeh blur corresponding to the tehcnique specified.</returns>
        public static BokehBlur ToBlurInstance(this BokehTechnique name)
        {
            switch (name)
            {
                case BokehTechnique.CircularGaussian:
                    return new GaussianBokeh();

                case BokehTechnique.HexagonalMcIntosh:
                    return new McIntoshBokeh();

                case BokehTechnique.HexagonalTripleRhombi:
                    return new TripleRhombiBokeh();

                default:
                    throw new ArgumentOutOfRangeException("Unknown bokeh technique: " + name);
            }
        }
    }
       
}