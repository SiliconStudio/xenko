// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Base implementation of <see cref="IColorLight"/>
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class ColorLightBase : IColorLight
    {
        protected ColorLightBase()
        {
            Color = new LightColorRgb();
        }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        [DataMember(-10)]
        [NotNull]
        public ILightColor Color { get; set; }

        /// <summary>
        /// Computes the color with intensity, result is in linear space.
        /// </summary>
        /// <returns>Gets the color of this light in linear space.</returns>
        public Color3 ComputeColor(float intensity)
        {
            return (Color != null ? Color.ComputeColor() : new Color3(1.0f)).ToLinear() * intensity;
        }

        public abstract bool Update(LightComponent lightComponent);
    }
}