// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Engine
{
    [DataConverter(AutoGenerate = true)]
    public sealed class LightShaftsComponent : EntityComponent
    {
        public static PropertyKey<LightShaftsComponent> Key = new PropertyKey<LightShaftsComponent>("Key", typeof(LightShaftsComponent));

        public LightShaftsComponent()
        {
            LightShaftsBoundingBoxes = new List<Mesh>();
        }

        //[DataMemberConvert]
        public List<Mesh> LightShaftsBoundingBoxes { get; set; }

        [DataMemberConvert]
        public Color3 Color { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}