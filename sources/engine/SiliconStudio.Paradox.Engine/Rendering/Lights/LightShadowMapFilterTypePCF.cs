// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Lights
{

    public enum LightShadowMapFilterTypePcfSize
    {
        Filter3x3,

        Filter5x5,

        Filter7x7,
    }


    /// <summary>
    /// No shadowmap filter.
    /// </summary>
    [DataContract("LightShadowMapFilterTypePcf")]
    [Display("PCF")]
    public class LightShadowMapFilterTypePcf : ILightShadowMapFilterType
    {
        public LightShadowMapFilterTypePcf()
        {
            FilterSize = LightShadowMapFilterTypePcfSize.Filter3x3;
        }

        /// <summary>
        /// Gets or sets the size of the filter.
        /// </summary>
        /// <value>The size of the filter.</value>
        [DataMember(10)]
        [DefaultValue(LightShadowMapFilterTypePcfSize.Filter3x3)]
        public LightShadowMapFilterTypePcfSize FilterSize { get; set; }

        public bool RequiresCustomBuffer()
        {
            return false;
        }
    }
}