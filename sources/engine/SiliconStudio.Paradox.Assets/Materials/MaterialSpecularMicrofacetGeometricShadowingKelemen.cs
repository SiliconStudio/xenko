// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Kelemen Geometric Shadowing.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetGeometricShadowingKelemen")]
    [Display("Kelemen")]
    public class MaterialSpecularMicrofacetGeometricShadowingKelemen : IMaterialSpecularMicrofacetGeometricShadowingFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetGeometricShadowingKelemen");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetGeometricShadowingKelemen;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetGeometricShadowingKelemen).GetHashCode();
        }
    }
}