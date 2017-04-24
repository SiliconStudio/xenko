// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Neumann Geometric Shadowing.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetVisibilityNeumann")]
    [Display("Neumann")]
    public class MaterialSpecularMicrofacetVisibilityNeumann : IMaterialSpecularMicrofacetVisibilityFunction
    {
        public ShaderSource Generate()
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetVisibilityNeumann");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetVisibilityNeumann;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetVisibilityNeumann).GetHashCode();
        }
    }
}
