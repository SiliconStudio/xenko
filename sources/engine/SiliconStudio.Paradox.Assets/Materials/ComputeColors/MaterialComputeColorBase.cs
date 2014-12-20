// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// Base implementation for <see cref="IMaterialComputeColor"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class MaterialComputeColorBase : IMaterialComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialComputeColorBase"/> class.
        /// </summary>
        protected MaterialComputeColorBase()
        {
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IMaterialComputeColor> GetChildren(object context = null)
        {
            return Enumerable.Empty<IMaterialComputeColor>();
        }

        public abstract ShaderSource GenerateShaderSource(MaterialShaderGeneratorContext shaderGeneratorContext);
    }
}