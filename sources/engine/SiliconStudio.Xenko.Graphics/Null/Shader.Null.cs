// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL
using System;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class Shader
    {
        private Shader(GraphicsDevice device, ShaderStage shaderStage, byte[] shaderBytecode)
            : base(device)
        {
            throw new NotImplementedException();
        }

        public ShaderReflection GetReflection()
        {
            throw new NotImplementedException();
        }
    }
} 
#endif
