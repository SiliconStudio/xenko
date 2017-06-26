// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The GGX Normal Distribution.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetNormalDistributionGGX")]
    [Display("GGX")]
    public class MaterialSpecularMicrofacetNormalDistributionGGX : IMaterialSpecularMicrofacetNormalDistributionFunction
    {
        public ShaderSource Generate(MaterialGeneratorContext context)
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
