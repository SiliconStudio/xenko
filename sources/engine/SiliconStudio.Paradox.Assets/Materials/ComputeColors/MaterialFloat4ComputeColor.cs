// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Materials.Processor.Visitors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialFloat4ComputeColor>))]
    [DataContract("MaterialFloat4ComputeColor")]
    [Display("Constant Vector")]
    public class MaterialFloat4ComputeColor : MaterialValueComputeColor<Vector4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloat4ComputeColor"/> class.
        /// </summary>
        public MaterialFloat4ComputeColor()
            : this(Vector4.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFloat4ComputeColor"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialFloat4ComputeColor(Vector4 value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Float4";
        }

        public override ShaderSource GenerateShaderSource(MaterialGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var key = (ParameterKey<Vector4>)context.GetParameterKey(Key ?? baseKeys.ValueBaseKey ?? MaterialKeys.GenericValueVector4);
            context.Parameters.Set(key, Value);
            return new ShaderClassSource("ComputeColorConstantLink", Key);
            // TODO: 
            // return new ShaderClassSource("ComputeColorFixed", MaterialUtility.GetAsShaderString(Value));
        }
    }
}
