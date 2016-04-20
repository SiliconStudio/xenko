// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    [DataContract("ComputeColor")]
    [Display("Color")]
    public class ComputeColor : ComputeValueBase<Color4>, IComputeColor
    {
        /// <summary>
        /// Gets or sets a value indicating whether to convert the texture in pre-multiplied alpha.
        /// </summary>
        /// <value><c>true</c> to convert the texture in pre-multiplied alpha.; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// If checked, The color values will be pre-multiplied by the alpha value.
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; }

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
            PremultiplyAlpha = true;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Color";
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var key = context.GetParameterKey(Key ?? baseKeys.ValueBaseKey ?? MaterialKeys.GenericValueColor4);

            // Store the color in Linear space
            var color =  baseKeys.IsColor ? Value.ToColorSpace(context.ColorSpace) : Value;
            if (PremultiplyAlpha)
                color = Color4.PremultiplyAlpha(color);
            
            if (key is ValueParameterKey<Color4>)
            {
                context.Parameters.Set((ValueParameterKey<Color4>)key, color);
            }
            else if (key is ValueParameterKey<Vector4>)
            {
                context.Parameters.Set((ValueParameterKey<Vector4>)key, color);
            }
            else if (key is ValueParameterKey<Color3>)
            {
                
                context.Parameters.Set((ValueParameterKey<Color3>)key, (Color3)color);
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
            // return new ShaderClassSource("ComputeColorFixed", MaterialUtility.GetAsShaderString(Value));
        }
    }
}
