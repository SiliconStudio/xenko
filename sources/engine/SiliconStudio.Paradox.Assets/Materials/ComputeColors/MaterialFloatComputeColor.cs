// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialFloatComputeColor>))]
    [DataContract("MaterialFloatNode")]
    [Display("Constant Float")]
    public class MaterialFloatComputeColor : MaterialConstantComputeColor<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloatComputeColor"/> class.
        /// </summary>
        public MaterialFloatComputeColor()
            : this(0.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloatComputeColor"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialFloatComputeColor(float value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Float";
        }

        public override ShaderSource GenerateShaderSource(MaterialShaderGeneratorContext shaderGeneratorContext)
        {
            if (Key != null)
            {
                // TODO constantValues.Set(Key, Value);
                return new ShaderClassSource("ComputeColorConstantFloatLink", Key);
            }

            return new ShaderClassSource("ComputeColorFixed", MaterialUtility.GetAsShaderString(Value));
        }
    }
}
