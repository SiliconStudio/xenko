// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class Shader : GraphicsResourceBase
    {
        internal ShaderStage stage;

        public static Shader New(GraphicsDevice device, ShaderStage shaderStage, byte[] shaderBytecode)
        {
            return new Shader(device, shaderStage, shaderBytecode);
        }
    }
}