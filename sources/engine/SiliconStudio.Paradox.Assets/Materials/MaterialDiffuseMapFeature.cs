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
    /// A Diffuse map for the diffuse material feature.
    /// </summary>
    [DataContract("MaterialDiffuseMapFeature")]
    [Display("Diffuse Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialDiffuseMapFeature : IMaterialDiffuseFeature
    {
        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Diffuse Map")]
        [DefaultValue(null)]
        public MaterialComputeColor DiffuseMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialDiffuseMapFeature() { DiffuseMap = new MaterialTextureComputeColor() };
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matDiffuse", DiffuseMap, MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue);
        }
    }
}