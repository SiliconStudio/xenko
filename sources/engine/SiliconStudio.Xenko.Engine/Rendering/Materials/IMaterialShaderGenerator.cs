// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// Defines the interface to generate the shaders for a <see cref="IMaterialFeature"/>
    /// </summary>
    public interface IMaterialShaderGenerator
    {
        /// <summary>
        /// Generates the shader.
        /// </summary>
        /// <param name="context">The context.</param>
        void Visit(MaterialGeneratorContext context);
    }
}