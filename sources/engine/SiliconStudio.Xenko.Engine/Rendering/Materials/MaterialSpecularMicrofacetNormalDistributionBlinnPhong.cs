// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The Blinn-Phong Normal Distribution.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetNormalDistributionBlinnPhong")]
    [Display("Blinn-Phong")]
    public class MaterialSpecularMicrofacetNormalDistributionBlinnPhong : IMaterialSpecularMicrofacetNormalDistributionFunction
    {
        public ShaderSource Generate(MaterialGeneratorContext context)
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetNormalDistributionBlinnPhong");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetNormalDistributionBlinnPhong;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetNormalDistributionBlinnPhong).GetHashCode();
        }
    }
}
