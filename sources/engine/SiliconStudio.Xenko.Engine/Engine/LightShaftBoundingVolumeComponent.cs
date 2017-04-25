// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    [Display("Light shaft bounding volume")]
    [DataContract("LightShaftBoundingVolumeComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftBoundingVolumeProcessor))]
    public class LightShaftBoundingVolumeComponent : ActivableEntityComponent
    {
        public Model Model { get; set; }

        public LightShaftComponent LightShaft { get; set; }
    }
}
