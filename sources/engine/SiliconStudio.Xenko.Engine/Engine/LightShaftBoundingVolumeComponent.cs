// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A bounding volume for light shafts to be rendered in, can take any <see cref="Model"/> as a volume
    /// </summary>
    [Display("Light Shaft Bounding Volume", Expand = ExpandRule.Always)]
    [DataContract("LightShaftBoundingVolumeComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftBoundingVolumeProcessor))]
    public class LightShaftBoundingVolumeComponent : ActivableEntityComponent
    {
        private Model model;
        private LightShaftComponent lightShaft;
        private bool enabled = true;

        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; EnabledChanged?.Invoke(this, null); }
        }

        /// <summary>
        /// The model used to define the bounding volume
        /// </summary>
        public Model Model
        {
            get { return model; }
            set { model = value; ModelChanged?.Invoke(this, null); }
        }

        /// <summary>
        /// The light shaft to which the bounding volume applies
        /// </summary>
        public LightShaftComponent LightShaft
        {
            get { return lightShaft; }
            set { lightShaft = value; LightShaftChanged?.Invoke(this, null); }
        }

        public event EventHandler LightShaftChanged;
        public event EventHandler ModelChanged;
        public event EventHandler EnabledChanged;
    }
}
