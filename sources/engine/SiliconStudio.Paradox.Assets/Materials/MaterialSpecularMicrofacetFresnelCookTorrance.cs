// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Fresnel using Cook-Torrance approximation.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetFresnelCookTorrance")]
    [Display("Cook-Torrance")]
    public class MaterialSpecularMicrofacetFresnelCookTorrance : IMaterialSpecularMicrofacetFresnelFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetFresnelCookTorrance");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetFresnelCookTorrance;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetFresnelCookTorrance).GetHashCode();
        }
    }
}