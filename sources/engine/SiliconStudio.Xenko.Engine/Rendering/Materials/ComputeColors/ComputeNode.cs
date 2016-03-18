// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// Base interface for all computer color nodes.
    /// </summary>
    [DataContract(Inherited = true)]
    [NonIdentifiable]
    public abstract class ComputeNode : IComputeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeNode"/> class.
        /// </summary>
        protected ComputeNode()
        {
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="context">The context to get the children.</param>
        /// <returns>The list of children.</returns>
        public virtual IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return Enumerable.Empty<ComputeNode>();
        }


        /// <summary>
        /// Generates the shader source equivalent for this node
        /// </summary>
        /// <returns>ShaderSource.</returns>
        public abstract ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys);
    }
}