// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Effects
{
    [DataConverter(AutoGenerate = true, ContentReference = true)]
    [DataSerializerGlobal(null, typeof(ContentReference<LightingConfigurationsSetData>))]
    public class LightingConfigurationsSet
    {
        public LightingConfigurationsSet()
        {
            Configs = null;
        }

        [DataMemberConvert]
        public LightingConfiguration[] Configs { get; set; }
    }
}
