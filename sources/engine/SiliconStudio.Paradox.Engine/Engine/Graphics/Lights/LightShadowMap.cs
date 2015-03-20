// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{

    [DataContract("LightShadowMapSplitMode")]
    public enum LightShadowMapSplitMode
    {
        Manual,

        Logarithmic,

        PSSM
    }


    /// <summary>
    /// A shadow map.
    /// </summary>
    [DataContract("LightShadowMap")]
    [Display("ShadowMap")]
    public class LightShadowMap : ILightShadow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMap"/> class.
        /// </summary>
        public LightShadowMap()
        {
            Enabled = false;
            Size = LightShadowMapSize.Medium;
            CascadeCount = LightShadowMapCascadeCount.TwoCascades;
            MinDistance = 0.0f;
            MaxDistance = 1.0f;
            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.00f;
            SplitMode = LightShadowMapSplitMode.Manual;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LightShadowMap"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DataMember(10)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the shadow map filtering.
        /// </summary>
        /// <value>The filter type.</value>
        [DataMember(20)]
        [DefaultValue(null)]
        public ILightShadowMapFilterType Filter { get; set; }

        /// <summary>
        /// Gets or sets the size of the shadowmap.
        /// </summary>
        /// <value>The size.</value>
        [DataMember(30)]
        [DefaultValue(LightShadowMapSize.Medium)]
        public LightShadowMapSize Size { get; set; }

        /// <summary>
        /// Gets or Sets the number of cascades for this shadow (valid only for directional lights)
        /// </summary>
        /// <value>The number of cascades for this shadow.</value>
        [DataMember(40)]
        [DefaultValue(LightShadowMapCascadeCount.TwoCascades)]
        public LightShadowMapCascadeCount CascadeCount { get; set; }

        [DataMemberIgnore]
        public LightShadowMapSplitMode SplitMode { get; set; }

        /// <summary>
        /// Gets or sets the minimum distance.
        /// </summary>
        /// <value>The minimum distance.</value>
        [DataMemberIgnore]
        public float MinDistance { get; set; }

        [DataMemberIgnore]
        public float MaxDistance { get; set; }

        [DataMemberIgnore]
        public float SplitDistance0 { get; set; }

        [DataMemberIgnore]
        public float SplitDistance1 { get; set; }

        [DataMemberIgnore]
        public float SplitDistance2 { get; set; }
        [DataMemberIgnore]
        public float SplitDistance3 { get; set; }

        [DataMemberIgnore]
        public bool Stabilized { get; set; }
    }
}