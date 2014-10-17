// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    // Serializers needed by Material system
    [DataSerializerGlobal(null, typeof(ContentReference<SamplerState>))]
    [DataSerializerGlobal(null, typeof(ContentReference<BlendState>))]
    [DataSerializerGlobal(null, typeof(ContentReference<RasterizerState>))]
    [DataSerializerGlobal(null, typeof(ContentReference<DepthStencilState>))]
    [DataConverter(AutoGenerate = true, ContentReference = true)]
    public class Material
    {
        public Material()
        {
            Parameters = new ParameterCollection();
        }

        [DataMemberConvert]
        public ParameterCollection Parameters { get; set; }
    }
}
