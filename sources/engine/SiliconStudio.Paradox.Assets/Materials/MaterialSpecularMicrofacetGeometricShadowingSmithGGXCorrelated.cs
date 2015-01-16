// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Smith-GGX Geometric Shadowing.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetGeometricShadowingSmithGGXCorrelated")]
    [Display("Smith-GGX Correlated")]
    public class MaterialSpecularMicrofacetGeometricShadowingSmithGGXCorrelated : IMaterialSpecularMicrofacetGeometricShadowingFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetGeometricShadowingSmithGGXCorrelated");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetGeometricShadowingSmithGGXCorrelated;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetGeometricShadowingSmithGGXCorrelated).GetHashCode();
        }
    }
}