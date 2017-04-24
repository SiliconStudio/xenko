// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Colors;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Base implementation of <see cref="IColorLight"/>
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class ColorLightBase : IColorLight
    {
        protected ColorLightBase()
        {
            Color = new ColorRgbProvider();
        }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        /// <value>The color.</value>
        /// <userdoc>The color emitted by the light.</userdoc>
        [DataMember(-10)]
        [NotNull]
        public IColorProvider Color { get; set; }

        /// <summary>
        /// Computes the color with intensity, result is in linear space.
        /// </summary>
        /// <returns>Gets the color of this light in linear space.</returns>
        public Color3 ComputeColor(ColorSpace colorSpace, float intensity)
        {
            var color = (Color != null ? Color.ComputeColor() : new Color3(1.0f));
            color = color.ToColorSpace(colorSpace) * intensity;
            return color;
        }

        public abstract bool Update(LightComponent lightComponent);
    }
}
