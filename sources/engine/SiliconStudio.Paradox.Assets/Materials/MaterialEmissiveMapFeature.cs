// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    [DataContract("MaterialEmissiveMapFeature")]
    [Display("Emissive Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialEmissiveMapFeature : IMaterialEmissiveFeature
    {
        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Emissive Map")]
        [DefaultValue(null)]
        public MaterialComputeColor EmissiveMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [Display("Intensity")]
        [DefaultValue(null)]
        public MaterialComputeColor Intensity { get; set; }

        public bool IsLightDependent
        {
            get
            {
                return false;
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matEmissive", EmissiveMap, MaterialKeys.EmissiveMap, MaterialKeys.EmissiveValue);
            context.SetStream("matEmissiveIntensity", Intensity, null, MaterialKeys.EmissiveIntensity);
            // TODO: Add shading model
            context.AddShading(this, new ShaderClassSource("TODOEmissive"));
        }

        public bool Equals(IMaterialShadingModelFeature other)
        {
            return other is MaterialEmissiveMapFeature;
        }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialEmissiveMapFeature()
                {
                    EmissiveMap = new MaterialTextureComputeColor(),
                    Intensity =  new MaterialFloatComputeColor(1.0f)
                };
            }
        }
    }
}