// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering.Materials
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
