// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialColorComputeColor>))]
    [DataContract("MaterialColorNode")]
    [Display("Constant Color")]
    public class MaterialColorComputeColor : MaterialConstantComputeColor<Color4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialColorComputeColor"/> class.
        /// </summary>
        public MaterialColorComputeColor()
            : this(Color4.Black)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialColorComputeColor"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialColorComputeColor(Color4 value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Color";
        }

        public override ShaderSource GenerateShaderSource(MaterialShaderGeneratorContext shaderGeneratorContext, ParameterKey baseKey)
        {
            if (Key != null)
            {
                // TODO constantValues.Set(Key, Value);
                return new ShaderClassSource("ComputeColorConstantColorLink", Key);
            }

            return new ShaderClassSource("ComputeColorFixed", MaterialUtility.GetAsShaderString(Value));
        }
    }
}
