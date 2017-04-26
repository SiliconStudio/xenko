// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Rendering.LightProbes;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("LightProbeComponent")]
    [Display("Light Probe", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(LightProbeProcessor))]
    [ComponentOrder(15000)]
    public class LightProbeComponent : EntityComponent
    {
        [Display(Browsable = false)]
        [NonIdentifiableCollectionItems]
        public FastList<Color3> Coefficients { get; set; }
    }
}
