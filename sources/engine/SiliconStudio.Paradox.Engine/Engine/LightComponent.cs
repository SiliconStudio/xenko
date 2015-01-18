// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract("ShadowMapFilterType")]
    public enum ShadowMapFilterType
    {
        Nearest = 0,
        PercentageCloserFiltering = 1,
        Variance = 2,
    }


    public interface ILightShadow
    {
        bool Enabled { get; set; }
    }

    [DataContract]
    public class LightShadowMap : ILightShadow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMap"/> class.
        /// </summary>
        public LightShadowMap()
        {
            Enabled = false;
            FilterType = ShadowMapFilterType.Nearest;
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
        [DefaultValue(ShadowMapFilterType.Nearest)]
        public ShadowMapFilterType FilterType { get; set; }

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

    public interface ILight
    {
    }

    [DataContract]
    public class LightDirectional : ILight
    {
    }

    [DataContract]
    public class LightSpot : ILight
    {
        public LightSpot()
        {
            SpotBeamAngle = 0;
            SpotFieldAngle = 0;
            DecayStart = 100.0f;
        }

        /// <summary>
        /// Gets or sets the decay start.
        /// </summary>
        /// <value>The decay start.</value>
        [DefaultValue(100.0f)]
        public float DecayStart { get; set; }

        /// <summary>Gets or sets the beam angle of the spot light.</summary>
        /// <value>The beam angle of the spot (in degrees between 0 and 90).</value>
        [DefaultValue(0)]
        public float SpotBeamAngle { get; set; }

        /// <summary>Gets or sets the spot field angle of the spot light.</summary>
        /// <value>The spot field angle of the spot (in degrees between 0 and 90).</value>
        [DefaultValue(0)]
        public float SpotFieldAngle { get; set; }
    }


    /// <summary>
    /// Add a light to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("LightComponent")]
    public sealed class LightComponent : EntityComponent
    {
        public static PropertyKey<LightComponent> Key = new PropertyKey<LightComponent>("Key", typeof(LightComponent));

        public LightComponent()
        {
            Color = new Color3(1.0f);
            Intensity = 1.0f;
            Enabled = true;
            Deferred = false;
            Direction = new Vector3(0, 0, -1);
            Layers = RenderLayers.RenderLayerAll;
            Type = new LightDirectional();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the light is enabled.
        /// </summary>
        [DataMember(0)]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember(10)]
        public Color3 Color { get; set; }

        /// <summary>
        /// Gets or sets the light intensity.
        /// </summary>
        /// <value>The light intensity.</value>
        [DataMember(20)]
        [DefaultValue(1.0f)]
        public float Intensity { get; set; }

        /// <summary>Gets or sets the light direction.</summary>
        /// <value>The light direction.</value>
        [DataMember(30)]
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets the type of the light.
        /// </summary>
        /// <value>The type of the light.</value>
        [DataMember(40)]
        [NotNull]
        public ILight Type { get; set; }

        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        [DataMember(50)]
        [DefaultValue(null)]
        public ILightShadow Shadow { get; set; }

        /// <summary>
        /// Get or sets the layers that the light influences
        /// </summary>
        /// <value>
        /// The layer mask.
        /// </value>
        [DataMember(60)]
        [DefaultValue(RenderLayers.RenderLayerAll)]
        public RenderLayers Layers { get; set; }

        /// <summary>Gets or sets a value indicating whether the light is deferred (if available).</summary>
        /// <value>true if light is deferred, false if not.</value>
        [DataMember(70)]
        [DefaultValue(true)]
        public bool Deferred { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}