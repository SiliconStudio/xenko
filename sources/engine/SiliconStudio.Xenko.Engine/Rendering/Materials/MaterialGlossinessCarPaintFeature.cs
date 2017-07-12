// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A smoothness map for the micro-surface material feature.
    /// </summary>
    [DataContract("MaterialGlossinessCarPaintFeature")]
    [Display("Car Paint Glossiness")]
    public class MaterialGlossinessCarPaintFeature : MaterialGlossinessMapFeature
    {
        public MaterialGlossinessCarPaintFeature()
        {
            GlossinessMap = new ComputeFloat(0.40f);
            ClearCoatGlossinessMap = new ComputeFloat(1.00f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGlossinessMapFeature"/> class.
        /// </summary>
        /// <param name="glossinessMap">The glossiness map.</param>
        public MaterialGlossinessCarPaintFeature(IComputeScalar metalFlakesGlossinessMap, IComputeScalar clearCoatGlossinessMap)
            : base(metalFlakesGlossinessMap)
        {
            ClearCoatGlossinessMap = clearCoatGlossinessMap;
        }

        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Clear Coat Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatGlossinessMap { get; set; }

        public override void MultipassGeneration(MaterialGeneratorContext context)
        {
            const int passCount = 2;
            context.SetMultiplePasses("CarPaint", passCount);
        }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            var passIndex = context.PassIndex;
            GlossinessMap.ClampFloat(0, 1);
            ClearCoatGlossinessMap.ClampFloat(0, 1);

            context.UseStream(MaterialShaderStage.Pixel, GlossinessStream.Stream);

            var computeColorSource = (passIndex == 0) 
                ? GlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue))
                : ClearCoatGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMap", Invert));
            mixin.AddComposition("glossinessMap", computeColorSource);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
        }
    }
}
