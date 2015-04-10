// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects.Shadows;

namespace SiliconStudio.Paradox.Effects.Lights
{

    [DataContract("LightShadowMapSplitMode")]
    public enum LightShadowMapSplitMode
    {
        Manual,

        Logarithmic,

        PSSM
    }

    public interface ILightShadowMap : ILightShadow
    {
        ILightShadowMapRenderer CreateRenderer(ILight light);
    }

    /// <summary>
    /// A shadow map.
    /// </summary>
    [DataContract("LightShadowMap")]
    [Display("ShadowMap")]
    public class LightShadowMap : ILightShadowMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMap"/> class.
        /// </summary>
        public LightShadowMap()
        {
            Enabled = false;
            Size = LightShadowMapSize.Medium;
            CascadeCount = LightShadowMapCascadeCount.FourCascades;
            MinDistance = 0.0f;
            MaxDistance = 1.0f;
            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.00f;
            SplitMode = LightShadowMapSplitMode.PSSM;
            StabilizationMode = LightShadowMapStabilizationMode.ProjectionSnapping;
            DepthBias = 1.0f;
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
        [DataMember(35)]
        public LightShadowImportance Importance { get; set; }

        /// <summary>
        /// Gets or Sets the number of cascades for this shadow (valid only for directional lights)
        /// </summary>
        /// <value>The number of cascades for this shadow.</value>
        [DataMember(40)]
        [DefaultValue(LightShadowMapCascadeCount.FourCascades)]
        public LightShadowMapCascadeCount CascadeCount { get; set; }

        /// <summary>
        /// Gets or sets the split mode.
        /// </summary>
        /// <value>The split mode.</value>
        [DataMember(50)]
        [DefaultValue(LightShadowMapSplitMode.PSSM)]
        public LightShadowMapSplitMode SplitMode { get; set; }

        [DataMember(55)]
        [DefaultValue(LightShadowMapStabilizationMode.ProjectionSnapping)]
        public LightShadowMapStabilizationMode StabilizationMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is filtering accross cascades.
        /// </summary>
        /// <value><c>true</c> if this instance is filtering accross cascades; otherwise, <c>false</c>.</value>
        [DataMember(57)]
        [DefaultValue(false)]
        public bool IsBlendingCascades { get; set; }

        /// <summary>
        /// Gets or sets the depth bias.
        /// </summary>
        /// <value>The bias.</value>
        [DataMember(58)]
        [DefaultValue(1.0f)]
        public float DepthBias { get; set; }

        /// <summary>
        /// Gets or sets the minimum distance.
        /// </summary>
        /// <value>The minimum distance.</value>
        [DataMember(60)]
        [DefaultValue(0.0f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float MinDistance { get; set; }

        [DataMember(70)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float MaxDistance { get; set; }

        [DataMember(80)]
        [DefaultValue(0.05f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance0 { get; set; }

        [DataMember(90)]
        [DefaultValue(0.15f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance1 { get; set; }

        [DataMember(100)]
        [DefaultValue(0.5f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance2 { get; set; }

        [DataMember(110)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance3 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LightShadowMap"/> is debug.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        [DataMember(130)]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        public ILightShadowMapRenderer CreateRenderer(ILight light)
        {
            if (light is LightDirectional)
            {
                return new LightDirectionalShadowMapRenderer();
            }

            return null;
        }
    }
}