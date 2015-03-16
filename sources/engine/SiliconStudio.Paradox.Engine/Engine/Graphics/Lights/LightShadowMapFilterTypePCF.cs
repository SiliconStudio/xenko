// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// No shadowmap filter.
    /// </summary>
    [DataContract("LightShadowMapFilterTypePcf")]
    [Display("PCF")]
    public class LightShadowMapFilterTypePcf : ILightShadowMapFilterType
    {
        public bool RequiresCustomBuffer()
        {
            return false;
        }
    }
}