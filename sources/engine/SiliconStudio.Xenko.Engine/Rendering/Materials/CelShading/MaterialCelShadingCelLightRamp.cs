// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Default Cel Shading ramp function applied
    /// </summary>
    [DataContract("MaterialCelShadingCelLightRamp")]
    [Display("Ramp")]
    public class MaterialCelShadingCelLightRamp : IMaterialCelShadingCelLightFunction
    {
        /// <summary>
        /// The texture Reference.
        /// </summary>
        /// <userdoc>
        /// The reference to the texture asset to use.
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(null)]
        [Display("Ramp Texture")]
        public Texture RampTexture { get; set; }

        public ShaderSource Generate(MaterialGeneratorContext context) // (ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            // If we haven't specified a texture use the default implementation
            if (RampTexture == null)
                return new ShaderClassSource("MaterialCelShadingCelLightDefault");

            context.Material.Parameters.Set(MaterialCelShadingCelLightRampKeys.CelShaderRamp, RampTexture);

            return new ShaderClassSource("MaterialCelShadingCelLightRamp");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialCelShadingCelLightRamp;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialCelShadingCelLightRamp).GetHashCode();
        }
    }

    public static partial class MaterialCelShadingCelLightRampKeys
    {
        public static readonly ObjectParameterKey<Texture> CelShaderRamp    = ParameterKeys.NewObject<Texture>();
        public static readonly ValueParameterKey<Color4> CelShaderRampValue = ParameterKeys.NewValue<Color4>();
    }
}
