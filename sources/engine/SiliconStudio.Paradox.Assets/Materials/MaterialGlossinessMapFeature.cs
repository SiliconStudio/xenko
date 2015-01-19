// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
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
    public class MaterialGlossinessMapFeature : IMaterialMicroSurfaceFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGlossinessMapFeature"/> class.
        /// </summary>
        public MaterialGlossinessMapFeature()
        {
            GlossinessMap = new MaterialTextureComputeScalar();
        }

        /// <summary>
        /// Gets or sets the smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IMaterialComputeScalar GlossinessMap { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            context.SetStream("matGlossiness", GlossinessMap, MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue);
        }
    }
}