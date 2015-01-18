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
        /// Initializes a new instance of the <see cref="MaterialSpecularMapFeature"/> class.
        /// </summary>
        public MaterialSpecularMapFeature()
        {
            IsEnergyConservative = true;
        }

        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        /// <value>The specular map.</value>
        [Display("Specular Map")]
        [DefaultValue(null)]
        public MaterialComputeColor SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the specular intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DefaultValue(null)]
        public MaterialComputeColor Intensity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is energy conservative.
        /// </summary>
        /// <value><c>true</c> if this instance is energy conservative; otherwise, <c>false</c>.</value>
        [DataMember(10)]
        [DefaultValue(true)]
        [Display("Is Energy Conservative?")]
        public bool IsEnergyConservative { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSpecularMapFeature()
                {
                    SpecularMap = new MaterialTextureComputeColor(),
                    Intensity = new MaterialFloatComputeColor(1.0f),
                };
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matSpecular", SpecularMap, MaterialKeys.SpecularMap, MaterialKeys.SpecularValue);
            context.SetStream("matSpecularIntensity", Intensity, null, MaterialKeys.SpecularIntensityValue);
        }
    }
}