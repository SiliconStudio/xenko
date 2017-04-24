// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Lights
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
        /// <userdoc>The size of the filter (size of the kernel).</userdoc>
        [DataMember(10)]
        [DefaultValue(LightShadowMapFilterTypePcfSize.Filter3x3)]
        public LightShadowMapFilterTypePcfSize FilterSize { get; set; }

        public bool RequiresCustomBuffer()
        {
            return false;
        }
    }
}
