// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Base class for a shadow map.
    /// </summary>
    [DataContract("LightShadowMap")]
    [Display("ShadowMap")]
    public abstract class LightShadowMap : ILightShadow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMap"/> class.
        /// </summary>
        protected LightShadowMap()
        {
            Enabled = false;
            Size = LightShadowMapSize.Medium;
            BiasParameters = new ShadowMapBiasParameters();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LightShadowMap"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DataMember(10)]
        [DefaultValue(false)]
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
        /// Gets the importance of the shadow. See remarks.
        /// </summary>
        /// <value>The shadow importance.</value>
        /// <returns>System.Single.</returns>
        /// <remarks>The higher the importance is, the higher the cost of shadow computation is costly</remarks>
        [DataMember(40)]
        public LightShadowImportance Importance { get; set; }


        /// <summary>
        /// Gets the bias parameters.
        /// </summary>
        /// <value>The bias parameters.</value>
        [DataMember(100)]
        [Display("Bias Parameters", AlwaysExpand = true)]
        public ShadowMapBiasParameters BiasParameters { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LightShadowMap"/> is debug.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        [DataMember(200)]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        public virtual int GetCascadeCount()
        {
            return 1;
        }

        /// <summary>
        /// Bias parameters used for shadow map.
        /// </summary>
        [DataContract("LightShadowMap.ShadowMapBiasParameters")]
        public sealed class ShadowMapBiasParameters
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ShadowMapBiasParameters"/> class.
            /// </summary>
            public ShadowMapBiasParameters()
            {
                DepthBias = 0.01f;
            }

            /// <summary>
            /// Gets or sets the depth bias used for shadow map comparison.
            /// </summary>
            /// <value>The bias.</value>
            [DataMember(10)]
            [DefaultValue(0.01f)]
            public float DepthBias { get; set; }

            /// <summary>
            /// Gets or sets the offset scale in world space unit along the surface normal.
            /// </summary>
            /// <value>The offset scale.</value>
            [DataMember(20)]
            [DefaultValue(0.0f)]
            public float NormalOffsetScale { get; set; }
        }
    }
}