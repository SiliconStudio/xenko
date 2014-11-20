// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Effects
{
    public class TessellationKeys
    {
        /// <summary>
        /// Desired maximum triangle size in screen space during tessellation.
        /// </summary>
        public static readonly ParameterKey<float> DesiredTriangleSize = ParameterKeys.New(12.0f);
    }
}