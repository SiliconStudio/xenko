// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// A standard shadow map.
    /// </summary>
    [DataContract("LightPointShadowMap")]
    [Display("Point ShadowMap")]
    public sealed class LightPointShadowMap : LightShadowMap
    {
        /// <summary>
        /// The type of shadow mapping technique to use for this point light
        /// </summary>
        [DefaultValue(LightPointShadowMapType.CubeMap)]
        public LightPointShadowMapType Type { get; set; } = LightPointShadowMapType.CubeMap;
    }
}