// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Lights
{

    public abstract class DirectLightBase : IDirectLight
    {
        protected DirectLightBase()
        {
            Color = new LightColorRgb();
        }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember(10)]
        [NotNull]
        public ILightColor Color { get; set; }

        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        [DataMember(200)]
        [DefaultValue(null)]
        public ILightShadow Shadow { get; set; }

        /// <summary>
        /// Computes the color with intensity, result is in linear space.
        /// </summary>
        /// <returns>Gets the color of this light in linear space.</returns>
        public Color3 ComputeColor(float intensity)
        {
            return (Color != null ? Color.ComputeColor() : new Color3(1.0f)).ToLinear() * intensity;
        }
    }

    /// <summary>
    /// A directional light.
    /// </summary>
    [DataContract("LightDirectional")]
    [Display("Directional")]
    public class LightDirectional : DirectLightBase
    {
        // TODO: Add support for disk based sun
    }
}