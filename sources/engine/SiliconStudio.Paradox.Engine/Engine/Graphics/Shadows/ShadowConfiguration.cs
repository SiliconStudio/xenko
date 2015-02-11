// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Lights;

namespace SiliconStudio.Paradox.Engine.Graphics.Shadows
{
    [DataContract("ShadowConfiguration")]
    public struct ShadowConfiguration
    {
        /// <summary>
        /// The type of light.
        /// </summary>
        /// <userdoc>
        /// The type of light that will cast shadows. Point light is not yet supported.
        /// </userdoc>
        [DataMember(10)]
        public LightType LightType;

        /// <summary>
        /// The number of shadows in this group.
        /// </summary>
        /// <userdoc>
        /// The number of shadows in this group.
        /// </userdoc>
        [DataMember(20)]
        public int ShadowCount;

        /// <summary>
        /// The number of cascade in this group.
        /// </summary>
        /// <userdoc>
        /// The number of cascades of each shadow map in this group
        /// </userdoc>
        [DataMember(30)]
        public int CascadeCount;

        /// <summary>
        ///  The type of filtering in this group.
        /// </summary>
        /// <userdoc>
        /// The shadow filtering of this group.
        /// </userdoc>
        [DataMember(40)]
        public LightShadowMapFilterType FilterType;
    }

    // TODO: array?
    [DataContract("ShadowConfigurationArray")]
    public class ShadowConfigurationArray
    {
        /// <summary>
        /// The groups of shadows. Each group contains only one type light, of filtering and a fixed number of cascades.
        /// </summary>
        /// <userdoc>
        /// The groups of shadowmaps. The groups will be processed at the same time.
        /// </userdoc>
        [DataMember(10)]
        public List<ShadowConfiguration> Groups;

        public ShadowConfigurationArray()
        {
            Groups = new List<ShadowConfiguration>();
        }
    }
}
