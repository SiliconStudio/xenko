// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

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
            GlossinessMap = new ComputeTextureScalar();
        }

        /// <summary>
        /// Gets or sets the smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1)]
        public IComputeScalar GlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialGlossinessMapFeature"/> is invert.
        /// </summary>
        /// <value><c>true</c> if invert; otherwise, <c>false</c>.</value>
        [Display("Invert")]
        [DefaultValue(false)]
        public bool Invert { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            if (GlossinessMap != null)
            {
                context.UseStream("matGlossiness");
                var computeColorSource = GlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMap", Invert));
                mixin.AddComposition("glossinessMap", computeColorSource);
                context.AddSurfaceShader(MaterialShaderStage.Pixel, mixin);
            }
        }
    }
}