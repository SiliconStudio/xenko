// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A Metalness map for the specular material feature.
    /// </summary>
    [DataContract("MaterialMetalnessCarPaintFeature")]
    [Display("Car Paint Metalness")]
    public class MaterialMetalnessCarPaintFeature : MaterialMetalnessMapFeature
    {
        public MaterialMetalnessCarPaintFeature()
        {
            MetalnessMap = new ComputeFloat(0.80f);
            ClearCoatMetalnessMap = new ComputeFloat(0.00f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGlossinessMapFeature"/> class.
        /// </summary>
        /// <param name="glossinessMap">The glossiness map.</param>
        public MaterialMetalnessCarPaintFeature(IComputeScalar metalFlakesMetalnessMap, IComputeScalar clearCoatMetalnessMap)
            : base(metalFlakesMetalnessMap)
        {
            ClearCoatMetalnessMap = clearCoatMetalnessMap;
        }

        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Clear Coat Metalness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatMetalnessMap { get; set; } = new ComputeFloat(0.0f);

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            IComputeScalar temporaryScalar = null;

            int passIndex = context.PassIndex % 2;

            context.MaterialPass.BlendState = new BlendStateDescription(Blend.One, Blend.One);

            if (passIndex == 1)
            {
                temporaryScalar = MetalnessMap;
                MetalnessMap = ClearCoatMetalnessMap;
            }

            base.GenerateShader(context);

            if (temporaryScalar != null)
                MetalnessMap = temporaryScalar;
        }      
    }
}
