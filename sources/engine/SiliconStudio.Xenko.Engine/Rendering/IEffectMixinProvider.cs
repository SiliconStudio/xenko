// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering
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