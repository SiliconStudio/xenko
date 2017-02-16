// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.LightProbes;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("LightProbeComponent")]
    [Display("Light Probe", Expand = ExpandRule.Once)]
    [ComponentOrder(15000)]
    public class LightProbeComponent : EntityComponent
    {
        public static readonly PropertyKey<LightProbeRuntimeData> RuntimeData = new PropertyKey<LightProbeRuntimeData>("RuntimeData", typeof(LightProbeComponent));

        [Display(Browsable = false)]
        [NonIdentifiableCollectionItems]
        public FastList<Color3> Coefficients { get; set; }
    }
}