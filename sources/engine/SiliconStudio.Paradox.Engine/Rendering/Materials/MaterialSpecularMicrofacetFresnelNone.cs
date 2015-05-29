// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// No Fresnel applied.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetFresnelNone")]
    [Display("None")]
    public class MaterialSpecularMicrofacetFresnelNone : IMaterialSpecularMicrofacetFresnelFunction
    {
        public ShaderSource Generate()
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