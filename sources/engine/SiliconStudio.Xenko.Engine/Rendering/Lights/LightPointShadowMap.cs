// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
