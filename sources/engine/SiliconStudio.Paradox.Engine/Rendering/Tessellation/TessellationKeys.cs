// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Tessellation
{
    public class TessellationKeys
    {
        /// <summary>
        /// Desired maximum triangle size in screen space during tessellation.
        /// </summary>
        public static readonly ParameterKey<float> DesiredTriangleSize = ParameterKeys.New(12.0f);

        /// <summary>
        /// The intensity of the smoothing for PN-based tessellation.
        /// </summary>
        public static readonly ParameterKey<Texture> SmoothingMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> SmoothingValue = ParameterKeys.New<float>(); 
    }
}