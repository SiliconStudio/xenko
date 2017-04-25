// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Engine
{
    [Display("Light Shaft", Expand = ExpandRule.Auto)]
    [DataContract("LightShaftComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftProcessor), ExecutionMode = ExecutionMode.All)]
    public class LightShaftComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Density of the volumetric fog generated for areas that are covered by this light
        /// </summary>
        public float DensityFactor { get; set; } = 0.01f;

        public float ExtinctionFactor { get; set; } = 0.001f;

        public float ExtinctionRatio { get; set; } = 0.9f;
        
        /// <summary>
        /// Number of samples taken per pixel
        /// </summary>
        /// <userdoc>
        /// Number of samples taken per pixel
        /// </userdoc>
        public int SampleCount { get; set; } = 16;
    }
}
