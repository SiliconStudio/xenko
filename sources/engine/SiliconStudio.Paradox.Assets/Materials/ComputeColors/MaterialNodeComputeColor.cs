// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    [DataContract("MaterialNodeComputeColor")]
    [Display("Constant Color")]
    public class MaterialNodeComputeColor : MaterialValueComputeNode<Color4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNodeComputeColor"/> class.
        /// </summary>
        public MaterialNodeComputeColor()
            : this(Color4.Black)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNodeComputeColor"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public MaterialNodeComputeColor(Color4 value)
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

            // Store the color in Linear space directly to avoid having to compute this at shader time
            var color = Value.ToLinear();
            
            if (key is ParameterKey<Color4>)
            {
                context.Parameters.Set((ParameterKey<Color4>)key, color);
            }
            else if (key is ParameterKey<Color3>)
            {
                
                context.Parameters.Set((ParameterKey<Color3>)key, (Color3)color);
            }
            else
            {
                context.Log.Error("Unexpected ParameterKey type [{0}]. Expecting a [Color3] or [Color4]", key.PropertyType);
            }
            UsedKey = key;

            return new ShaderClassSource("ComputeColorConstantColorLink", key);
            // return new ShaderClassSource("ComputeColorFixed", MaterialUtility.GetAsShaderString(Value));
        }
    }
}
