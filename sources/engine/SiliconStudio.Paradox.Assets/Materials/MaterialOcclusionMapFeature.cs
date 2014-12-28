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
    /// An occlusion map for the occlusion material feature.
    /// </summary>
    [DataContract("MaterialOcclusionMapFeature")]
    [Display("Occlusion Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialOcclusionMapFeature : IMaterialOcclusionFeature
    {
        /// <summary>
        /// Gets or sets the occlusion map.
        /// </summary>
        /// <value>The occlusion map.</value>
        [Display("Occlusion Map")]
        [DefaultValue(null)]
        [DataMember(10)]
        public MaterialComputeColor AmbientOcclusionMap { get; set; }

        /// <summary>
        /// Gets or sets the cavity map.
        /// </summary>
        /// <value>The cavity map.</value>
        [Display("Cavity Map")]
        [DefaultValue(null)]
        [DataMember(20)]
        public MaterialComputeColor CavityMap { get; set; }

        /// <summary>
        /// Gets or sets the diffuse cavity influence.
        /// </summary>
        /// <value>The diffuse cavity.</value>
        [Display("Diffuse Cavity")]
        [DefaultValue(null)]
        [DataMember(30)]
        [DataRange(0.0f, 1.0f, 0.01f)]
        public MaterialComputeColor DiffuseCavity { get; set; }

        /// <summary>
        /// Gets or sets the specular cavity.
        /// </summary>
        /// <value>The specular cavity.</value>
        [Display("Specular Cavity")]
        [DefaultValue(null)]
        [DataMember(40)]
        [DataRange(0.0f, 1.0f, 0.01f)]
        public MaterialComputeColor SpecularCavity { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialOcclusionMapFeature()
                {
                    AmbientOcclusionMap = new MaterialTextureComputeColor(),
                    CavityMap = new MaterialTextureComputeColor(),
                    DiffuseCavity = new MaterialFloatComputeColor(1.0f),
                    SpecularCavity = new MaterialFloatComputeColor(1.0f),
                };
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matAmbientOcclusion", AmbientOcclusionMap, MaterialKeys.AmbientOcclusionMap, MaterialKeys.AmbientOcclusionValue);
            context.SetStream("matCavity", CavityMap, MaterialKeys.CavityMap, MaterialKeys.CavityValue);
            context.SetStream("matCavityDiffuse", DiffuseCavity, null, MaterialKeys.CavityDiffuseValue);
            context.SetStream("matCavitySpecular", SpecularCavity, null, MaterialKeys.CavitySpecularValue);
        }
    }
}