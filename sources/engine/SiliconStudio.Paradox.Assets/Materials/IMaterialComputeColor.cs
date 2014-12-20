// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Base interface for all nodes in the material tree
    /// </summary>
    public interface IMaterialComputeColor
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="context">The shaderGeneratorContext to get the children.</param>
        /// <returns>The list of children.</returns>
        IEnumerable<IMaterialComputeColor> GetChildren(object context = null);

        /// <summary>
        /// Generates the shader source equivalent for this node
        /// </summary>
        /// <returns>ShaderSource.</returns>
        ShaderSource GenerateShaderSource(MaterialShaderGeneratorContext shaderGeneratorContext);
    }
}