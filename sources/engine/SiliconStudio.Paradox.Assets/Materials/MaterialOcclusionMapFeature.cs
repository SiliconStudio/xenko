// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// An occlusion map for the occlusion material feature.
    /// </summary>
    [DataContract("MaterialOcclusionMapFeature")]
    [Display("Occlusion Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialOcclusionMapFeature : MaterialFeatureBase, IMaterialOcclusionFeature
    {
        /// <summary>
        /// Gets or sets the occlusion map.
        /// </summary>
        /// <value>The occlusion map.</value>
        [Display("Occlusion Map")]
        [DefaultValue(null)]
        [DataMember(10)]
        [MaterialStream("matAmbientOcclusion", MaterialStreamType.Float, "Material.AmbientOcclusionMap")]
        public IMaterialComputeColor AmbientOcclusionMap { get; set; }

        /// <summary>
        /// Gets or sets the cavity map.
        /// </summary>
        /// <value>The cavity map.</value>
        [Display("Cavity Map")]
        [DefaultValue(null)]
        [DataMember(20)]
        [MaterialStream("matCavity", MaterialStreamType.Float, "Material.CavityMap")]
        public IMaterialComputeColor CavityMap { get; set; }

        /// <summary>
        /// Gets or sets the diffuse cavity influence.
        /// </summary>
        /// <value>The diffuse cavity.</value>
        [Display("Diffuse Cavity")]
        [DefaultValue(null)]
        [DataMember(30)]
        [DataRange(0.0f, 1.0f, 0.01f)]
        [MaterialStream("matCavityDiffuse", MaterialStreamType.Float)]
        public IMaterialComputeColor DiffuseCavity { get; set; }

        /// <summary>
        /// Gets or sets the specular cavity.
        /// </summary>
        /// <value>The specular cavity.</value>
        [Display("Specular Cavity")]
        [DefaultValue(null)]
        [DataMember(40)]
        [DataRange(0.0f, 1.0f, 0.01f)]
        [MaterialStream("matCavitySpecular", MaterialStreamType.Float)]
        public IMaterialComputeColor SpecularCavity { get; set; }

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
    }
}