// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Add a light to an <see cref="Entity"/>, that will be used during rendering.
    /// </summary>
    [DataContract("LightComponent")]
    [Display(120, "Light")]
    public sealed class LightComponent : EntityComponent
    {
        public static PropertyKey<LightComponent> Key = new PropertyKey<LightComponent>("Key", typeof(LightComponent));

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponent"/> class.
        /// </summary>
        public LightComponent()
        {
            Type = new LightDirectional();
            Color = new LightColorRgb();
            Intensity = 1.0f;
            Layers = RenderLayers.All;
        }

        /// <summary>
        /// Gets or sets the type of the light.
        /// </summary>
        /// <value>The type of the light.</value>
        [DataMember(10)]
        [NotNull]
        public ILight Type { get; set; }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember(20)]
        [NotNull]
        public ILightColor Color { get; set; }

        /// <summary>
        /// Gets or sets the light intensity.
        /// </summary>
        /// <value>The light intensity.</value>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        public float Intensity { get; set; }

        /// <summary>
        /// Get or sets the layers that the light influences
        /// </summary>
        /// <value>
        /// The layer mask.
        /// </value>
        [DataMember(60)]
        [DefaultValue(RenderLayers.All)]
        public RenderLayers Layers { get; set; }
        
        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        [DataMember(50)]
        [DefaultValue(null)]
        public ILightShadow Shadow { get; set; }

        /// <summary>
        /// Computes the color with intensity, result is in linear space.
        /// </summary>
        /// <returns>Gets the color of this light in linear space.</returns>
        public Color3 ComputeColorWithIntensity()
        {
            return (Color != null ? Color.ComputeColor() : new Color3(1.0f)).ToLinear() * Intensity;
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}