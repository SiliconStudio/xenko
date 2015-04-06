// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public class LightShaderGroup
    {
        public LightShaderGroup() : this(null)
        {
        }

        public LightShaderGroup(ShaderSource shaderSource)
        {
            ShaderSource = shaderSource;
            Parameters = new ParameterCollection();
        }

        public bool IsEnvironementLightGroup { get; set; }

        public ShaderSource ShaderSource { get; set; }

        public ParameterCollection Parameters { get; private set; }
    }
}