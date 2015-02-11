// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A shadow map.
    /// </summary>
    [DataContract("LightShadowMap")]
    public class LightShadowMap : ILightShadow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMap"/> class.
        /// </summary>
        public LightShadowMap()
        {
            Enabled = false;
            FilterType = LightShadowMapFilterType.Nearest;
            MaxSize = 512;
            MinSize = 512;
            CascadeCount = 1;
            NearDistance = 1.0f;
            FarDistance = 100000.0f;
            BleedingFactor = 0.0f;
            MinVariance = 0.0f;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LightShadowMap"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the shadow map filtering.
        /// </summary>
        /// <value>The filter type.</value>
        [DefaultValue(LightShadowMapFilterType.Nearest)]
        public LightShadowMapFilterType FilterType { get; set; }

        /// <summary>Gets or Sets the maximium size (in pixel) of one cascade of the shadow map.</summary>
        /// <value>The maximum size of the shadow map.</value>
        [DefaultValue(512)]
        public int MaxSize { get; set; }

        /// <summary>Gets or Sets the minimum size (in pixel) of one cascade of the shadow map.</summary>
        /// <value>The minimum size of the shadow map.</value>
        [DefaultValue(512)]
        public int MinSize { get; set; }

        /// <summary>Gets or Sets the number of cascades for this shadow.</summary>
        /// <value>The number of cascades for this shadow.</value>
        [DefaultValue(1)]
        public int CascadeCount { get; set; }

        /// <summary>Gets or Sets the near plane distance of the shadow.</summary>
        /// <value>The near plane distance of the shadow.</value>
        [DefaultValue(1.0f)]
        public float NearDistance { get; set; }

        /// <summary>Gets or Sets the far plane distance of the shadow.</summary>
        /// <value>The far plane distance of the shadow.</value>
        [DefaultValue(100000.0f)]
        public float FarDistance { get; set; }

        /// <summary>
        /// Gets or sets the bleeding factor of the variance shadow map.
        /// </summary>
        /// <value>The bleeding factor.</value>
        [DefaultValue(0.0f)]
        public float BleedingFactor { get; set; }

        /// <summary>
        /// Gets or sets the minimal value of the variance of the variance shadow map.
        /// </summary>
        /// <value>The minimal variance.</value>
        [DefaultValue(0.0f)]
        public float MinVariance { get; set; }
    }
}