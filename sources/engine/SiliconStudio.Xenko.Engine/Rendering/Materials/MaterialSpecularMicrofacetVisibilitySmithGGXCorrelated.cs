// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Smith-GGX Geometric Shadowing.
    /// </summary>
    [DataContract("MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated")]
    [Display("Smith-GGX Correlated")]
    public class MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated : IMaterialSpecularMicrofacetVisibilityFunction
    {
        public ShaderSource Generate(MaterialGeneratorContext context)
        {
            return new ShaderClassSource("MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialSpecularMicrofacetVisibilitySmithGGXCorrelated).GetHashCode();
        }
    }
}
