// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A light color described by a rgb color
    /// </summary>
    [DataContract("LightColorRgb")]
    [Display("RGB")]
    public class LightColorRgb : ILightColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightColorRgb"/> class.
        /// </summary>
        public LightColorRgb()
        {
            Color = new Color3(1.0f);
        }

        /// <summary>
        /// Gets or sets the light color in rgb.
        /// </summary>
        /// <value>The color.</value>
        [DataMember(10)]
        public Color3 Color { get; set; }

        public Color3 ComputeColor()
        {
            return Color;
        }
    }
}