// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A smoothness map for the car paint micro-surface material feature.
    /// </summary>
    [DataContract("MaterialGlossinessCarPaintFeature")]
    [Display("Car Paint Glossiness")]
    public class MaterialGlossinessCarPaintFeature : MaterialGlossinessMapFeature
    {
        public MaterialGlossinessCarPaintFeature()
        {
            // Use default metal flakes normal map as a mask
            var metalFlakesNormalMap = new ComputeTextureScalar
            {
                Texture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("7e2761d1-ef86-420a-b7a7-a0ed1c16f9bb"), "XenkoCarPaintMetalFlakesNM"),
                Scale = new Vector2(128, 128),
                UseRandomTexCoordinates = true
            };

            BasePaintGlossinessMap = new ComputeBinaryScalar(new ComputeFloat(0.00f), metalFlakesNormalMap, BinaryOperator.Multiply);
            
            // 'Boost' glossiness
            GlossinessMap = new ComputeBinaryScalar(new ComputeFloat(2.00f), metalFlakesNormalMap, BinaryOperator.Multiply);

            ClearCoatGlossinessMap = new ComputeFloat(1.00f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialGlossinessCarPaintFeature"/> class.
        /// </summary>
        /// <param name="metalFlakesGlossinessMap">The metal flakes glossiness map.</param>
        /// <param name="clearCoatGlossinessMap">The clear coat glossiness map.</param>
        /// <param name="baseGlossinessMap">The base layer glossiness map (the layer below the metal flakes one).</param>
        public MaterialGlossinessCarPaintFeature(IComputeScalar metalFlakesGlossinessMap, IComputeScalar clearCoatGlossinessMap, IComputeScalar baseGlossinessMap)
            : base(metalFlakesGlossinessMap)
        {
            ClearCoatGlossinessMap = clearCoatGlossinessMap;
            BasePaintGlossinessMap = baseGlossinessMap;
        }

        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Clear Coat Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets the base paint smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Base Paint Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar BasePaintGlossinessMap { get; set; }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            var isMetalFlakesPass = (context.PassIndex == 0);

            GlossinessMap.ClampFloat(0, 1);
            ClearCoatGlossinessMap.ClampFloat(0, 1);

            context.UseStream(MaterialShaderStage.Pixel, GlossinessStream.Stream);

            // Set the source depending of the index
            var computeColorSource = (isMetalFlakesPass) 
                ? GlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue))
                : ClearCoatGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
            
            var mixin = new ShaderMixinSource();
            
            mixin.Mixins.Add(new ShaderClassSource((isMetalFlakesPass) ? "MaterialSurfaceGlossinessMapMetalFlakes" : "MaterialSurfaceGlossinessMap", Invert));
            mixin.AddComposition("glossinessMap", computeColorSource);

            if (isMetalFlakesPass)
            {
                var baseShininessComputeColorSource = BasePaintGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
                mixin.AddComposition("baseGlossinessMap", baseShininessComputeColorSource);
            }
            

            context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
        }
    }
}
