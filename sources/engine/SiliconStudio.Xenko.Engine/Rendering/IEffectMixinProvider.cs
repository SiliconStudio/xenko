// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines the interface to provide an effect mixin for a <see cref="CameraRendererMode"/>.
    /// </summary>
    public interface IEffectMixinProvider
    {
        /// <summary>
        /// Generates the shader source used for rendering.
        /// </summary>
        /// <returns>ShaderSource.</returns>
        ShaderSource GenerateShaderSource();
    }

    [DataContract("DefaultEffectMixinProvider")]
    public class DefaultEffectMixinProvider : IEffectMixinProvider
    {
        private readonly ShaderSource shaderSource;

        public DefaultEffectMixinProvider(string name)
        {
            shaderSource = new ShaderClassSource(name);
        }

        public ShaderSource GenerateShaderSource()
        {
            return shaderSource;
        }
    }
}
