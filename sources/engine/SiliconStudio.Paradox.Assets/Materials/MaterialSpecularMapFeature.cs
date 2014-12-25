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
    /// A Specular map for the specular material feature.
    /// </summary>
    [DataContract("MaterialSpecularMapFeature")]
    [Display("Specular Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSpecularMapFeature : IMaterialSpecularFeature
    {
        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        /// <value>The specular map.</value>
        [Display("Specular Map")]
        [DefaultValue(null)]
        public MaterialComputeColor SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DefaultValue(null)]
        public MaterialComputeColor Intensity { get; set; }

        /// <summary>
        /// Gets or sets the fresnel.
        /// </summary>
        /// <value>The fresnel.</value>
        [DefaultValue(null)]
        public MaterialComputeColor Fresnel { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSpecularMapFeature()
                {
                    SpecularMap = new MaterialTextureComputeColor(),
                    Intensity = new MaterialFloatComputeColor(1.0f),
                    Fresnel = new MaterialFloatComputeColor(1.0f),
                };
            }
        }

        public void GenerateShader(MaterialShaderGeneratorContext context)
        {
            context.SetStream("matSpecular", SpecularMap, MaterialKeys.SpecularMap, MaterialKeys.SpecularValue);
            context.SetStream("matSpecularIntensity", Intensity, null, MaterialKeys.SpecularIntensityValue);
            context.SetStream("matSpecularFresnel", Fresnel, null, MaterialKeys.SpecularFresnelValue);
        }
    }
}