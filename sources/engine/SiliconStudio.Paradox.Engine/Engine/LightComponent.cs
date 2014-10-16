// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;

using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.DataModel;
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

    /// <summary>
    /// Add a light to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataConverter(AutoGenerate = true)]
    [DataContract("LightComponent")]
    public sealed class LightComponent : EntityComponent
    {
        public static PropertyKey<LightComponent> Key = new PropertyKey<LightComponent>("Key", typeof(LightComponent));

        public LightComponent()
        {
            Intensity = 1.0f;
            ShadowMapFilterType = ShadowMapFilterType.Nearest;
            Enabled = true;
            Deferred = false;
            Layers = RenderLayers.RenderLayerAll;
            SpotBeamAngle = 0;
            SpotFieldAngle = 0;
            ShadowMap = false;
            ShadowMapMaxSize = 512;
            ShadowMapMinSize = 512;
            ShadowMapCascadeCount = 1;
            ShadowNearDistance = 1.0f;
            ShadowFarDistance = 100000.0f;
            BleedingFactor = 0.0f;
            MinVariance = 0.0f;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the light is enabled.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>Gets or sets a value indicating whether the light is deferred (if available).</summary>
        /// <value>true if light is deferred, false if not.</value>
        [DataMemberConvert]
        public bool Deferred { get; set; }

        /// <summary>
        /// Gets or sets the light type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [DataMemberConvert]
        public LightType Type { get; set; }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        [DataMemberConvert]
        public Color3 Color { get; set; }

        /// <summary>
        /// Gets or sets the light intensity.
        /// </summary>
        /// <value>
        /// The light intensity.
        /// </value>
        [DataMemberConvert]
        [DefaultValue(1.0f)]
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets the decay start.
        /// </summary>
        /// <value>
        /// The decay start.
        /// </value>
        [DataMemberConvert]
        public float DecayStart { get; set; }

        /// <summary>Gets or sets the light direction.</summary>
        /// <value>The light direction.</value>
        [DataMemberConvert]
        public Vector3 LightDirection { get; set; }

        /// <summary>Gets or sets the beam angle of the spot light.</summary>
        /// <value>The beam angle of the spot (in degrees between 0 and 90).</value>
        [DataMemberConvert]
        [DefaultValue(0)]
        public float SpotBeamAngle { get; set; }

        /// <summary>Gets or sets the spot field angle of the spot light.</summary>
        /// <value>The spot field angle of the spot (in degrees between 0 and 90).</value>
        [DataMemberConvert]
        [DefaultValue(0)]
        public float SpotFieldAngle { get; set; }

        /// <summary>Gets or Sets a value indicating if the light cast shadows.</summary>
        /// <value>True if the ligh generates a shadowmap, false otherwise.</value>
        [DataMemberConvert]
        [DefaultValue(false)]
        public bool ShadowMap { get; set; }

        /// <summary>Gets or Sets the maximium size (in pixel) of one cascade of the shadow map.</summary>
        /// <value>The maximum size of the shadow map.</value>
        [DataMemberConvert]
        [DefaultValue(512)]
        public int ShadowMapMaxSize { get; set; }

        /// <summary>Gets or Sets the minimum size (in pixel) of one cascade of the shadow map.</summary>
        /// <value>The minimum size of the shadow map.</value>
        [DataMemberConvert]
        [DefaultValue(512)]
        public int ShadowMapMinSize { get; set; }

        /// <summary>Gets or Sets the number of cascades for this shadow.</summary>
        /// <value>The number of cascades for this shadow.</value>
        [DataMemberConvert]
        [DefaultValue(1)]
        public int ShadowMapCascadeCount { get; set; }

        /// <summary>Gets or Sets the near plane distance of the shadow.</summary>
        /// <value>The near plane distance of the shadow.</value>
        [DataMemberConvert]
        [DefaultValue(1.0f)]
        public float ShadowNearDistance { get; set; }

        /// <summary>Gets or Sets the far plane distance of the shadow.</summary>
        /// <value>The far plane distance of the shadow.</value>
        [DataMemberConvert]
        [DefaultValue(100000.0f)]
        public float ShadowFarDistance { get; set; }

        /// <summary>
        /// Gets or sets the shadow map filtering.
        /// </summary>
        /// <value>The filter type.</value>
        [DataMemberConvert]
        [DefaultValue(ShadowMapFilterType.Nearest)]
        public ShadowMapFilterType ShadowMapFilterType { get; set; }

        /// <summary>
        /// Gets or sets the bleeding factor of the variance shadow map.
        /// </summary>
        /// <value>The bleeding factor.</value>
        [DataMemberConvert]
        [DefaultValue(0.0f)]
        public float BleedingFactor { get; set; }

        /// <summary>
        /// Gets or sets the minimal value of the variance of the variance shadow map.
        /// </summary>
        /// <value>The minimal variance.</value>
        [DataMemberConvert]
        [DefaultValue(0.0f)]
        public float MinVariance { get; set; }

        /// <summary>
        /// Get or sets the layers that the light influences
        /// </summary>
        /// <value>
        /// The layer mask.
        /// </value>
        [DataMemberConvert]
        [DefaultValue(RenderLayers.RenderLayerAll)]
        public RenderLayers Layers { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}