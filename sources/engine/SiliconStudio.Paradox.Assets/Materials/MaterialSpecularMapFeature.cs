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
    /// A Specular map for the specular material feature.
    /// </summary>
    [DataContract("MaterialSpecularMapFeature")]
    [Display("Specular Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSpecularMapFeature : MaterialFeatureBase, IMaterialSpecularFeature
    {
        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        /// <value>The specular map.</value>
        [Display("Specular Map")]
        [DefaultValue(null)]
        [MaterialStream("matSpecular", MaterialStreamType.Float3)]
        public IMaterialComputeColor SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the intensity.
        /// </summary>
        /// <value>The intensity.</value>
        [DefaultValue(null)]
        [MaterialStream("matSpecularIntensity", MaterialStreamType.Float)]
        public IMaterialComputeColor Intensity { get; set; }

        /// <summary>
        /// Gets or sets the fresnel.
        /// </summary>
        /// <value>The fresnel.</value>
        [DefaultValue(null)]
        [MaterialStream("matSpecularFresnel", MaterialStreamType.Float)]
        public IMaterialComputeColor Fresnel { get; set; }

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
    }
}