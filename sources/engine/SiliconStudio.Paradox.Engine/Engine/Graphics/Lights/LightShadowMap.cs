// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects.Shadows;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A shadow map.
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
        /// Gets or sets a value indicating whether this <see cref="LightShadowMap"/> is debug.
        /// </summary>
        /// <value><c>true</c> if debug; otherwise, <c>false</c>.</value>
        [DataMember(200)]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        public virtual ILightShadowMapRenderer CreateRenderer(ILight light)
        {
            return null;
        }

        public virtual int GetCascadeCount()
        {
            return 1;
        }
    }


    /// <summary>
    /// A standard shadow map.
    /// </summary>
    [DataContract("LightStandardShadowMap")]
    [Display("Standard ShadowMap")]
    public class LightStandardShadowMap : LightShadowMap
    {
    }


    [DataContract]
    public abstract class LightDirectionalPartitionMode
    {
        
    }

    /// <summary>
    /// Manual partition. This class cannot be inherited.
    /// </summary>
    [DataContract("LightDirectionalPartitionManual")]
    [Display("Manual")]
    public sealed class LightDirectionalPartitionManual : LightDirectionalPartitionMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightDirectionalManualPartitionMode"/> class.
        /// </summary>
        public LightDirectionalPartitionManual()
        {
            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.00f;
        }

        [DataMember(10)]
        [DefaultValue(0.05f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance0 { get; set; }

        [DataMember(20)]
        [DefaultValue(0.15f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance1 { get; set; }

        [DataMember(30)]
        [DefaultValue(0.5f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance2 { get; set; }

        [DataMember(40)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float SplitDistance3 { get; set; }
    }

    [DataContract("LightDirectionalPartitionLogarithmic")]
    [Display("Logarithmic")]
    public sealed class LightDirectionalPartitionLogarithmic : LightDirectionalPartitionMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightDirectionalPartitionLogarithmic"/> class.
        /// </summary>
        public LightDirectionalPartitionLogarithmic()
        {
            PSSMFactor = 0.5f;
        }


        [DataMember(10)]
        [DefaultValue(0.5f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        [Display("PSSM Factor")]
        public float PSSMFactor { get; set; }
    }

    [DataContract]
    public abstract class LightDirectionalDepthRangeMode
    {
    }


    [DataContract("LightDirectionalDepthRangeManual")]
    [Display("Manual")]
    public sealed class LightDirectionalDepthRangeManual : LightDirectionalDepthRangeMode
    {
        public LightDirectionalDepthRangeManual()
        {
            MinDistance = 0.0f;
            MaxDistance = 1.0f;
            IsBlendingCascades = true;
        }

        /// <summary>
        /// Gets or sets the minimum distance.
        /// </summary>
        /// <value>The minimum distance.</value>
        [DataMember(10)]
        [DefaultValue(0.0f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float MinDistance { get; set; }

        /// <summary>
        /// Gets or sets the maximum distance.
        /// </summary>
        /// <value>The maximum distance.</value>
        [DataMember(20)]
        [DefaultValue(1.0f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public float MaxDistance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is filtering accross cascades.
        /// </summary>
        /// <value><c>true</c> if this instance is filtering accross cascades; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(true)]
        public bool IsBlendingCascades { get; set; }
    }

    [DataContract("LightDirectionalDepthRangeAuto")]
    [Display("Auto")]
    public sealed class LightDirectionalDepthRangeAuto : LightDirectionalDepthRangeMode
    {
    }

    /// <summary>
    /// A directional shadow map.
    /// </summary>
    [DataContract("LightDirectionalShadowMap")]
    [Display("Directional ShadowMap")]
    public class LightDirectionalShadowMap : LightShadowMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMap"/> class.
        /// </summary>
        public LightDirectionalShadowMap()
        {
            CascadeCount = LightShadowMapCascadeCount.FourCascades;


            DepthRangeMode = new LightDirectionalDepthRangeAuto();
            PartitionMode = new LightDirectionalPartitionLogarithmic();

            StabilizationMode = LightShadowMapStabilizationMode.ProjectionSnapping;
            DepthBias = 0.001f;
        }

        /// <summary>
        /// Gets or Sets the number of cascades for this shadow (valid only for directional lights)
        /// </summary>
        /// <value>The number of cascades for this shadow.</value>
        [DataMember(50)]
        [DefaultValue(LightShadowMapCascadeCount.FourCascades)]
        public LightShadowMapCascadeCount CascadeCount { get; set; }

        [DataMember(60)]
        [DefaultValue(LightShadowMapStabilizationMode.ProjectionSnapping)]
        public LightShadowMapStabilizationMode StabilizationMode { get; set; }

        /// <summary>
        /// Gets or sets the depth range mode.
        /// </summary>
        /// <value>The depth range mode.</value>
        [DataMember(80)]
        [NotNull]
        public LightDirectionalDepthRangeMode DepthRangeMode { get; set; }

        /// <summary>
        /// Gets or sets the partition mode.
        /// </summary>
        /// <value>The partition mode.</value>
        [DataMember(90)]
        [NotNull]
        public LightDirectionalPartitionMode PartitionMode { get; set; }

        /// <summary>
        /// Gets or sets the depth bias.
        /// </summary>
        /// <value>The bias.</value>
        [DataMember(100)]
        [DefaultValue(0.001f)]
        public float DepthBias { get; set; }

        public override ILightShadowMapRenderer CreateRenderer(ILight light)
        {
            if (light is LightDirectional)
            {
                return new LightDirectionalShadowMapRenderer();
            }

            return null;
        }

        public override int GetCascadeCount()
        {
            return (int)CascadeCount;
        }
    }
}