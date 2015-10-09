// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Images
{

    /// <summary>
    /// Struct LuminanceResult
    /// </summary>
    public struct LuminanceResult
    {
        public LuminanceResult(float averageLuminance, Texture localTexture)
            : this()
        {
            AverageLuminance = averageLuminance;
            LocalTexture = localTexture;
        }

        public float AverageLuminance { get; set; }

        public Texture LocalTexture { get; set; }
    }
}