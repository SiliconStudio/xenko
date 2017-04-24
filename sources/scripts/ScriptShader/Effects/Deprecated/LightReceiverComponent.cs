// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Engine
{
    public class LightReceiverComponent : EntityComponent
    {
        public static readonly PropertyKey<LightReceiverComponent> Key = new PropertyKey<LightReceiverComponent>("Key", typeof(LightReceiverComponent));
        public static readonly PropertyKey<bool> ForcePerPixelLighting = new PropertyKey<bool>("ForcePerPixelLighting", typeof(LightReceiverComponent));

        private readonly TrackingCollection<LightComponent> lightComponents = new TrackingCollection<LightComponent>();

        /// <summary>
        /// Additional lights applied on this object.
        /// </summary>
        [DataMemberConvert]
        public IList<LightComponent> LightComponents { get { return lightComponents; } }
    }
}
