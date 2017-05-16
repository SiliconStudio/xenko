// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Some util function relevant to the depth-of-field effect.
    /// </summary>
    class DoFUtil
    {
        /// <summary>
        /// Creates an array with uniform weight along one direction of the blur. 
        /// </summary>
        /// <param name="count">Number of taps from the center (included) along one direction.</param>
        /// <returns>The array with uniform weights.</returns>
        public static float[] GetUniformWeightBlurArray(int count)
        {
            // Total number of taps
            var tapNumber = 2 * count - 1;
            var uniformWeight = 1f / tapNumber;
            float[] result = new float[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = uniformWeight;
            }
            return result;
        }
    }
}
