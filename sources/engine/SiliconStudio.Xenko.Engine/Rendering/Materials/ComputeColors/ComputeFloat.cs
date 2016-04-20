// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A float compute color.
    /// </summary>
    [DataContract("ComputeFloat")]
    [Display("Float")]
    public class ComputeFloat : ComputeValueBase<float>, IComputeScalar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeFloat"/> class.
        /// </summary>
        public ComputeFloat()
            : this(0.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeFloat"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ComputeFloat(float value)
            : base(value)
        {
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var key = (ValueParameterKey<float>)context.GetParameterKey(Key ?? baseKeys.ValueBaseKey ?? MaterialKeys.GenericValueFloat);
            context.Parameters.Set(key, Value);
            UsedKey = key;

            return new ShaderClassSource("ComputeColorConstantFloatLink", key);
        }
    }
}
