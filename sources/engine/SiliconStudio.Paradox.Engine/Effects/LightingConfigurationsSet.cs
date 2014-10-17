// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Effects
{
    [DataConverter(AutoGenerate = true, ContentReference = true)]
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
