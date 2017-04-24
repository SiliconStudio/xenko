// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public class Light
    {
        public Light()
        {
            Parameters = new ParameterCollection("Light Parameters");
            LightColor = new Color3(1.0f, 1.0f, 1.0f);
            LightShaderType = LightShaderType.DiffuseSpecularPixel;
        }

        public LightShaderType LightShaderType { get; set; }

        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        /// <value>
        /// The color of the light.
        /// </value>
        public Color3 LightColor
        {
            get { return Parameters.TryGet(LightKeys.LightColor); }
            set { Parameters.Set(LightKeys.LightColor, value); }
        }
    }
}
