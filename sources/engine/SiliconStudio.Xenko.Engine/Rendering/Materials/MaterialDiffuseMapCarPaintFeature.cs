// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A Diffuse map for the diffuse material feature.
    /// </summary>
    [DataContract("MaterialDiffuseMapCarPaintFeature")]
    [Display("Car Paint Diffuse Map")]
    public class MaterialDiffuseMapCarPaintFeature : MaterialDiffuseMapFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapCarPaintFeature"/> class.
        /// </summary>
        public MaterialDiffuseMapCarPaintFeature()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapCarPaintFeature"/> class.
        /// </summary>
        /// <param name="diffuseMap">The diffuse map.</param>
        public MaterialDiffuseMapCarPaintFeature(IComputeColor diffuseMap)
            : base(diffuseMap)
        {

        }

        /// <summary>
        /// Gets or sets the diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Metal Flakes Diffuse Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor MetalFlakesDiffuseMap { get; set; }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            bool isMetalFlakesPass = context.PassIndex == 0;

            //// Clear coat does not need a diffuse pass
            //if (!isMetalFlakesPass)
            //{
            //    return;
            //}

            if (DiffuseMap != null)
            {
                Vector4 diffuseMin = Vector4.Zero;
                Vector4 diffuseMax = Vector4.One;
                DiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);
                MetalFlakesDiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);

                var computeColorSource = DiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
                var metalFlakesComputeColorSource = MetalFlakesDiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource((isMetalFlakesPass) ? "MaterialSurfaceDiffuseCarPaint" : "MaterialSurfaceDiffuse"));
                mixin.AddComposition("diffuseMap", computeColorSource);

                if (isMetalFlakesPass)
                    mixin.AddComposition("metalFlakesDiffuseMap", metalFlakesComputeColorSource);

                context.UseStream(MaterialShaderStage.Pixel, DiffuseStream.Stream);
                context.UseStream(MaterialShaderStage.Pixel, ColorBaseStream.Stream);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
            }
        }
    }
}
