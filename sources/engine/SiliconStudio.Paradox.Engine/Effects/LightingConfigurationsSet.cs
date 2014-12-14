// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Effects
{
    [DataSerializerGlobal(typeof(ReferenceSerializer<LightingConfigurationsSet>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<LightingConfigurationsSet>))]
    [DataContract]
    public class LightingConfigurationsSet
    {
        public LightingConfigurationsSet()
        {
            Configs = null;
        }

        public LightingConfiguration[] Configs { get; set; }
    }
}
