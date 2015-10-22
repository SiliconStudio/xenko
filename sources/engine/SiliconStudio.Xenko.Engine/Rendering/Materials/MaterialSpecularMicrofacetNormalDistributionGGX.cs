// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// The GGX Normal Distribution.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetNormalDistributionGGX")]
    [Display("GGX")]
    public class MaterialSpecularMicrofacetNormalDistributionGGX : IMaterialSpecularMicrofacetNormalDistributionFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetNormalDistributionGGX");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetNormalDistributionGGX;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetNormalDistributionGGX).GetHashCode();
        }
    }
}