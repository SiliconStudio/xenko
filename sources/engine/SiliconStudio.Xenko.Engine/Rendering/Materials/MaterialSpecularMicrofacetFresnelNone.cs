// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// No Fresnel applied.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetFresnelNone")]
    [Display("None")]
    public class MaterialSpecularMicrofacetFresnelNone : IMaterialSpecularMicrofacetFresnelFunction
    {
        public ShaderSource Generate(MaterialGeneratorContext context)
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetFresnelNone");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetFresnelNone;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetFresnelNone).GetHashCode();
        }
    }
}
