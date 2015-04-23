// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    [DataContract("ComputeColor")]
    [Display("Color")]
    public class ComputeColor : ComputeValueBase<Color4>, IComputeColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeColor"/> class.
        /// </summary>
        public ComputeColor()
            : this(Color4.Black)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeColor"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ComputeColor(Color4 value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Color";
        }

        public override ShaderSource GenerateShaderSource(MaterialGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var key = context.GetParameterKey(Key ?? baseKeys.ValueBaseKey ?? MaterialKeys.GenericValueColor4);

            // Store the color in Linear space
            var color = Value.ToLinear();
            
            if (key is ParameterKey<Color4>)
            {
                context.Parameters.Set((ParameterKey<Color4>)key, color);
            }
            else if (key is ParameterKey<Vector4>)
            {
                context.Parameters.Set((ParameterKey<Vector4>)key, color);
            }
            else if (key is ParameterKey<Color3>)
            {
                
                context.Parameters.Set((ParameterKey<Color3>)key, (Color3)color);
            }
            else if (key is ParameterKey<Vector3>)
            {

                context.Parameters.Set((ParameterKey<Vector3>)key, (Vector3)color);
            }
            else
            {
                context.Log.Error("Unexpected ParameterKey [{0}] for type [{0}]. Expecting a [Vector3/Color3] or [Vector4/Color4]", key, key.PropertyType);
            }
            UsedKey = key;

            return new ShaderClassSource("ComputeColorConstantColorLink", key);
            // return new ShaderClassSource("ComputeColorFixed", MaterialUtility.GetAsShaderString(Value));
        }
    }
}
