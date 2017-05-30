// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
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
