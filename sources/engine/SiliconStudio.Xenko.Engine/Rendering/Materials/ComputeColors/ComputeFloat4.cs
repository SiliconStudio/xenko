// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    [DataContract("ComputeFloat4")]
    [Display("Float4")]
    public class ComputeFloat4 : ComputeValueBase<Vector4>, IComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeFloat4"/> class.
        /// </summary>
        public ComputeFloat4()
            : this(Vector4.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeFloat4"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ComputeFloat4(Vector4 value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Float4";
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var key = context.GetParameterKey(Key ?? baseKeys.ValueBaseKey ?? MaterialKeys.GenericValueVector4);

            // Store the color in Linear space
            var color = Value;

            // Convert from Vector4 to (Color4|Vector4|Color3|Vector3)
            if (key is ValueParameterKey<Color4>)
            {
                context.Parameters.Set((ValueParameterKey<Color4>)key, (Color4)color);
            }
            else if (key is ValueParameterKey<Vector4>)
            {
                context.Parameters.Set((ValueParameterKey<Vector4>)key, color);
            }
            else if (key is ValueParameterKey<Color3>)
            {

                context.Parameters.Set((ValueParameterKey<Color3>)key, (Color3)(Vector3)color);
            }
            else if (key is ValueParameterKey<Vector3>)
            {

                context.Parameters.Set((ValueParameterKey<Vector3>)key, (Vector3)color);
            }
            else
            {
                context.Log.Error("Unexpected ParameterKey [{0}] for type [{0}]. Expecting a [Vector3/Color3] or [Vector4/Color4]", key, key.PropertyType);
            }
            UsedKey = key;

            return new ShaderClassSource("ComputeColorConstantColorLink", key);
        }
    }
}
