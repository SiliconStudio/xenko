// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class EffectProgram
    {
        private EffectProgram(GraphicsDevice device, IEnumerable<Shader> shaders)
            : base(device)
        {
            throw new NotImplementedException();
        }

        public object GetInputSignature()
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<ShaderStage, ShaderReflection>[] GetReflection()
        {
            throw new NotImplementedException();
        }
    }
}
 
#endif
