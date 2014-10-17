// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Data
{
    [ContentSerializer(typeof(DataContentSerializer<EffectData>))]
    [DataContract]
    public class EffectData
    {
        public static readonly ParameterKey<ShaderMixinSource> Tessellation = ParameterKeys.New<ShaderMixinSource>();
        public static readonly ParameterKey<bool> TessellationAEN = ParameterKeys.New(false);
        public static readonly ParameterKey<bool> NeedAlphaBlending = ParameterKeys.New(false);

        public EffectData()
        {
            Parameters = new ParameterCollection();
        }

        public EffectData(string name) : this()
        {
            Name = name;
        }

        public ParameterCollection Parameters { get; set; }

        public string Name { get; set; }
        public string Part { get; set; }
    }
}