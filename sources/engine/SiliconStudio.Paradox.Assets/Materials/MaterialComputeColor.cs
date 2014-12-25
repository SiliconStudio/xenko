// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Base interface for all computer color nodes.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class MaterialComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialComputeColor"/> class.
        /// </summary>
        protected MaterialComputeColor()
        {
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="context">The shaderGeneratorContext to get the children.</param>
        /// <returns>The list of children.</returns>
        public virtual IEnumerable<MaterialComputeColor> GetChildren(object context = null)
        {
            return Enumerable.Empty<MaterialComputeColor>();
        }


        /// <summary>
        /// Generates the shader source equivalent for this node
        /// </summary>
        /// <returns>ShaderSource.</returns>
        public abstract ShaderSource GenerateShaderSource(MaterialShaderGeneratorContext shaderGeneratorContext, MaterialComputeColorKeys baseKeys);
    }

}