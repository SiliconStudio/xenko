// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A Diffuse map for the car paint diffuse material feature.
    /// </summary>
    [DataContract("MaterialDiffuseMapCarPaintFeature")]
    [Display("Car Paint Diffuse Map")]
    public class MaterialDiffuseMapCarPaintFeature : MaterialDiffuseMapFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapCarPaintFeature"/> class.
        /// </summary>
        public MaterialDiffuseMapCarPaintFeature()
        {
            MetalFlakesDiffuseMap = new ComputeColor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialDiffuseMapCarPaintFeature"/> class.
        /// </summary>
        /// <param name="diffuseMap">The diffuse map.</param>
        /// <param name="metalFlakesDiffuseMap">The metal flakes diffuse map.</param>
        public MaterialDiffuseMapCarPaintFeature(IComputeColor diffuseMap, IComputeColor metalFlakesDiffuseMap)
            : base(diffuseMap)
        {
            MetalFlakesDiffuseMap = metalFlakesDiffuseMap;
        }

        /// <summary>
        /// Gets or sets the metal flakes diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Metal Flakes Diffuse Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor MetalFlakesDiffuseMap { get; set; }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            // If the current pass is not the metal flakes one, use the default shader
            var isMetalFlakesPass = (context.PassIndex == 0);
            if (!isMetalFlakesPass)
            {
                base.GenerateShader(context);
                return;
            }
            
            var diffuseMin = Vector4.Zero;
            var diffuseMax = Vector4.One;
            DiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);
            MetalFlakesDiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);

            var computeColorSource = DiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
            var metalFlakesComputeColorSource = MetalFlakesDiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceDiffuseCarPaint"));
            mixin.AddComposition("metalFlakesDiffuseMap", metalFlakesComputeColorSource);
            mixin.AddComposition("diffuseMap", computeColorSource);

            context.UseStream(MaterialShaderStage.Pixel, DiffuseStream.Stream);
            context.UseStream(MaterialShaderStage.Pixel, ColorBaseStream.Stream);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
        }
    }
}
