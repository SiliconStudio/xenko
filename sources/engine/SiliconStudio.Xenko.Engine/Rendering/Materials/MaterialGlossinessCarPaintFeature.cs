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
        public IComputeScalar ClearCoatGlossinessMap { get; set; } = new ComputeFloat(1.0f);

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            IComputeScalar temporaryScalar = null;

            int passIndex = context.PassIndex % 2;
            
            if (passIndex == 1)
            {
                temporaryScalar = GlossinessMap;
                GlossinessMap = ClearCoatGlossinessMap;
            }

            base.GenerateShader(context);

            if (temporaryScalar != null)
                GlossinessMap = temporaryScalar;

            if (passIndex == 0)
            {
                context.MaterialPass.BlendState = BlendStates.Additive;
            }
            else if (passIndex == 1)
            {
                context.MaterialPass.BlendState = new BlendStateDescription(Blend.Zero, Blend.SourceColor) { RenderTarget0 = { AlphaSourceBlend = Blend.One, AlphaDestinationBlend = Blend.Zero } };
            }
        }
    }
}
