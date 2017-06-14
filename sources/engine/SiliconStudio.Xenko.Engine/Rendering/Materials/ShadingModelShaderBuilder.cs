// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Helper class to build the <see cref="ShaderSource"/> for the shading model of a <see cref="IMaterialShadingModelFeature"/>.
    /// </summary>
    public class ShadingModelShaderBuilder
    {
        public List<ShaderSource> ShaderSources { get; } = new List<ShaderSource>();

        /// <summary>
        /// Shaders that needs to be mixed on top of MaterialSurfaceLightingAndShading.
        /// </summary>
        public List<ShaderClassSource> LightDependentExtraModels { get; } = new List<ShaderClassSource>();

        public ShaderSource LightDependentSurface { get; set; }
    }
}