// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects
{
    [DataConverter(AutoGenerate = true)]
    [DataContract("ShadowConfiguration")]
    public struct ShadowConfiguration
    {
        /// <summary>
        /// The number of shadows in this group.
        /// </summary>
        [DataMemberConvert]
        [DataMember(10)]
        public int ShadowCount;

        /// <summary>
        /// The number of cascade in this group.
        /// </summary>
        [DataMemberConvert]
        [DataMember(20)]
        public int CascadeCount;

        /// <summary>
        ///  The type of filtering in this group.
        /// </summary>
        [DataMemberConvert]
        [DataMember(30)]
        public ShadowMapFilterType FilterType;
    }

    // TODO: array?
    [DataConverter(AutoGenerate = true)]
    [DataContract("ShadowConfigurationArray")]
    public class ShadowConfigurationArray
    {
        [DataMemberConvert]
        [DataMember(10)]
        public List<ShadowConfiguration> Groups;

        public ShadowConfigurationArray()
        {
            Groups = new List<ShadowConfiguration>();
        }
    }
}
