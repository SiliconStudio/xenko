// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Fresnel for glass materials.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetFresnelThinGlass")]
    [Display("Thin Glass")]
    public class MaterialSpecularMicrofacetFresnelThinGlass : IMaterialSpecularMicrofacetFresnelFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetFresnelThinGlass");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetFresnelThinGlass;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetFresnelThinGlass).GetHashCode();
        }
    }
}
