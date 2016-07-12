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
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialCelShadingCelLightDefault");
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
