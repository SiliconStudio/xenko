// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// Fresnel using Schlick approximation.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetFresnelSchlick")]
    [Display("Schlick")]
    public class MaterialSpecularMicrofacetFresnelSchlick : IMaterialSpecularMicrofacetFresnelFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetFresnelSchlick");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetFresnelSchlick;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetFresnelSchlick).GetHashCode();
        }
    }
}