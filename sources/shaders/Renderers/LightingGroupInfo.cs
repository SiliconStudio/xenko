// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Renderers
{
    public class LightingGroupInfo
    {
        public static readonly PropertyKey<LightingGroupInfo> Key = new PropertyKey<LightingGroupInfo>("LightingGroupInfoKey", typeof(LightingGroupInfo));

        // TODO: Remove this from this class
        /// <summary>
        /// The lighting parameters of this effect.
        /// </summary>
        public List<LightingUpdateInfo> LightingParameters { get; set; }

        // TODO: Remove this from this class
        /// <summary>
        /// The shadow parameters of this effect
        /// </summary>
        public List<ShadowUpdateInfo> ShadowParameters { get; set; }

        // TODO: Remove this from this class
        /// <summary>
        /// A flag stating if the LightingParameters should be updated.
        /// </summary>
        public bool IsLightingSetup { get; set; }


        public static LightingGroupInfo GetOrCreate(Effect effect)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            LightingGroupInfo value;
            if (!effect.Tags.TryGetValue(Key, out value))
            {
                value = new LightingGroupInfo();
                effect.Tags.Add(Key, value);
            }
            return value;
        }
    }
}