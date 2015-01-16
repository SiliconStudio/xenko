// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Schlick-Beckmann Geometric Shadowing.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetGeometricShadowingSmithSchlickBeckmann")]
    [Display("Schlick-Beckmann")]
    public class MaterialSpecularMicrofacetGeometricShadowingSmithSchlickBeckmann : IMaterialSpecularMicrofacetGeometricShadowingFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetGeometricShadowingSmithSchlickBeckmann");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetGeometricShadowingSmithSchlickBeckmann;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetGeometricShadowingSmithSchlickBeckmann).GetHashCode();
        }
    }
}