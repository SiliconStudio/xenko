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
    /// A smoothness map for the micro-surface material feature.
    /// </summary>
    [DataContract("MaterialGlossinessMapFeature")]
    [Display("Glossiness Map")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialGlossinessMapFeature : IMaterialMicroSurfaceFeature
    {
        /// <summary>
        /// Gets or sets the smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Glossiness Map")]
        [DefaultValue(null)]
        public MaterialComputeColor GlossinessMap { get; set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialGlossinessMapFeature()
                {
                    GlossinessMap = new MaterialTextureComputeColor() { Channel = TextureChannel.R }
                };
            }
        }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matGlossiness", GlossinessMap, MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue);
        }
    }
}