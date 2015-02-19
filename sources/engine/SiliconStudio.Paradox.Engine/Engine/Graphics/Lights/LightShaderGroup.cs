// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    public class LightShaderGroup
    {
        public LightShaderGroup(ShaderSource shaderSource)
        {
            if (shaderSource == null) throw new ArgumentNullException("shaderSource");
            ShaderSource = shaderSource;
            Parameters = new ParameterCollection();
        }

        public ShaderSource ShaderSource { get; private set; }

        public ParameterCollection Parameters { get; private set; }
    }
}