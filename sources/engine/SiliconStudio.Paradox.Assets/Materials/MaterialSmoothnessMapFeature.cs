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
    /// A smoothness map for the micro-surface material feature.
    /// </summary>
    [DataContract("MaterialSmoothnessMapFeature")]
    [Display("Smoothness Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialSmoothnessMapFeature : MaterialFeatureBase, IMaterialMicroSurfaceFeature
    {
        /// <summary>
        /// Gets or sets the smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Smoothness Map")]
        [DefaultValue(null)]
        [MaterialStream("matSmoothness", MaterialStreamType.Float)]
        public IMaterialComputeColor SmoothnessMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialSmoothnessMapFeature() { SmoothnessMap = new MaterialTextureComputeColor() };
            }
        }
    }
}