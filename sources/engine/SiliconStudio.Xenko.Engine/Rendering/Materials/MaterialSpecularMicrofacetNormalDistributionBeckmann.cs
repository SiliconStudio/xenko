// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// The Beckmann Normal Distribution.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetNormalDistributionBeckmann")]
    [Display("Beckmann")]
    public class MaterialSpecularMicrofacetNormalDistributionBeckmann : IMaterialSpecularMicrofacetNormalDistributionFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetNormalDistributionBeckmann");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetNormalDistributionBeckmann;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetNormalDistributionBeckmann).GetHashCode();
        }
    }
}