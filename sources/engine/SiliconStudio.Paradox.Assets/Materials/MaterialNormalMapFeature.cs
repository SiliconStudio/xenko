// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// The normal map for a surface material feature.
    /// </summary>
    [DataContract("MaterialNormalMapFeature")]
    [Display("Normal Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialNormalMapFeature : IMaterialSurfaceFeature
    {
        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        /// <value>The normal map.</value>
        [Display("Normal Map")]
        [DefaultValue(null)]
        public MaterialComputeColor NormalMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialNormalMapFeature()
                {
                    NormalMap = new MaterialTextureComputeColor() // TODO: handle xy vs xyz
                };
            }
        }

        public void GenerateShader(MaterialShaderGeneratorContext context)
        {
            // handle xy vs xyz shaders
            context.SetStream("matNormal", NormalMap, MaterialKeys.NormalMap, MaterialKeys.NormalValue);
        }
    }
}