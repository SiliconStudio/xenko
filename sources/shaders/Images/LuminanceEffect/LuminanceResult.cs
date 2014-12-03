// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{

    /// <summary>
    /// Struct LuminanceResult
    /// </summary>
    public struct LuminanceResult
    {
        public Texture Texture { get; set; }

        public float AverageLuminance { get; set; }

    }
}