// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Default Cel Shading ramp function applied
    /// </summary>
    [DataContract("MaterialCelShadingCelLightDefault")]
    [Display("Default")]
    public class MaterialCelShadingCelLightDefault : IMaterialCelShadingCelLightFunction
    {
        [DataMember(5)]
        [Display("Black and White")]
        public bool IsBlackAndWhite { get; set; } = false;

        public ShaderSource Generate(MaterialGeneratorContext context)
        {
            return new ShaderClassSource("MaterialCelShadingCelLightDefault", IsBlackAndWhite);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialCelShadingCelLightDefault;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialCelShadingCelLightDefault).GetHashCode();
        }
    }
}
