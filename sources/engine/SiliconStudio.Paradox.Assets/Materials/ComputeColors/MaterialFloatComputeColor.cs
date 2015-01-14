// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A float compute color.
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<MaterialFloatComputeColor>))]
    [DataContract("MaterialFloatComputeColor")]
    [Display("Constant Float")]
    [DebuggerDisplay("CompputeColor Float")]
    public class MaterialFloatComputeColor : MaterialValueComputeColor<float>
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

        public override ShaderSource GenerateShaderSource(MaterialGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var key = (ParameterKey<float>)context.GetParameterKey(Key ?? baseKeys.ValueBaseKey ?? MaterialKeys.GenericValueFloat);
            context.Parameters.Set(key, Value);
            UsedKey = key;

            return new ShaderClassSource("ComputeColorConstantFloatLink", Key ?? baseKeys.ValueBaseKey);
        }
    }
}
