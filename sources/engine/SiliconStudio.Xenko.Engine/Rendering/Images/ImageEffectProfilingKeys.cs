// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    public class ImageEffectProfilingKeys
    {
        public static readonly ProfilingKey PostEffect = new ProfilingKey("PostProcessingEffects");

        public static readonly ProfilingKey AmbientOcclusion = new ProfilingKey(PostEffect, "AmbientOcclusion");

        public static readonly ProfilingKey DepthOfField = new ProfilingKey(PostEffect, "DepthOfField");

        public static readonly ProfilingKey AverageLumiance = new ProfilingKey(PostEffect, "AverageLumiance");

        public static readonly ProfilingKey Bloom = new ProfilingKey(PostEffect, "Bloom");

        public static readonly ProfilingKey LightStreak = new ProfilingKey(PostEffect, "LightStreak");

        public static readonly ProfilingKey LensFlare = new ProfilingKey(PostEffect, "LensFlare");

        public static readonly ProfilingKey HybridFxaa = new ProfilingKey(PostEffect, "HybridFXAA");

        public static readonly ProfilingKey Fxaa = new ProfilingKey(PostEffect, "FXAA");

        public static readonly ProfilingKey ColorTransformGroup = new ProfilingKey(PostEffect, "ColorTransformGroup");

        public static readonly ProfilingKey BrightFilter = new ProfilingKey(PostEffect, "BrightFilter");
    }
}
