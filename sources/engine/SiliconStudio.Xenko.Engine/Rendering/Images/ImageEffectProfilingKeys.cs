// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public class ImageEffectProfilingKeys
    {
        public static readonly ProfilingKey PostEffect = new ProfilingKey("PostProcessingEffects", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey AmbientOcclusion = new ProfilingKey(PostEffect, "AmbientOcclusion", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey DepthOfField = new ProfilingKey(PostEffect, "DepthOfField", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey AverageLumiance = new ProfilingKey(PostEffect, "AverageLumiance", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey Bloom = new ProfilingKey(PostEffect, "Bloom", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey LightStreak = new ProfilingKey(PostEffect, "LightStreak", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey LensFlare = new ProfilingKey(PostEffect, "LensFlare", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey HybridFxaa = new ProfilingKey(PostEffect, "HybridFXAA", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey Fxaa = new ProfilingKey(PostEffect, "FXAA", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey ColorTransformGroup = new ProfilingKey(PostEffect, "ColorTransformGroup", ProfilingKeyFlags.GpuProfiling);

        public static readonly ProfilingKey BrightFilter = new ProfilingKey(PostEffect, "BrightFilter", ProfilingKeyFlags.GpuProfiling);
    }
}
