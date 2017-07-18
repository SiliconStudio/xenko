// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;
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
    [DataContract("MaterialClearCoatFeature")]
    [Display("Clear Coat")]
    [CategoryOrder(5, "Diffuse")]
    [CategoryOrder(10, "Surface")]
    [CategoryOrder(15, "Micro Surface")]
    [CategoryOrder(20, "Specular")]
    public class MaterialClearCoatFeature : MaterialFeature, IMaterialClearCoatFeature
    {
        /// <summary>
        /// Gets or sets the metal flakes diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        [Display("Metal Flakes Diffuse Map", "Diffuse")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor MetalFlakesDiffuseMap { get; set; }

        /// <summary>
        /// Gets or sets the normal map used for the clear coat layer.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map.
        /// </userdoc>
        [DataMember(110)]
        [Display("Orange Peel Normal Map", "Surface")]
        [NotNull]
        public IComputeColor ClearCoatLayerNormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2) and offset by (-1,-1) the normal map.
        /// </summary>
        /// <value><c>true</c> if scale and offset this normal map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale the XY by (2,2) and offset by (-1,-1). Required to unpack unsigned values of [0..1] to signed coordinates of [-1..+1].
        /// </userdoc>
        [DataMember(120)]
        [DefaultValue(true)]
        [Display("Scale & Offset", "Surface")]
        public bool ScaleAndBiasOrangePeel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be sqrt(1 - x*x - y*y).
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// The Z component of the normal vector will be calculated from X and Y assuming Z = sqrt(1 - x*x - y*y).
        /// </userdoc>
        [DataMember(130)]
        [DefaultValue(false)]
        [Display("Reconstruct Z", "Surface")]
        public bool IsXYNormalOrangePeel { get; set; }

        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Clear Coat Glossiness Map", "Micro Surface")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets the base paint smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        [Display("Base Paint Glossiness Map", "Micro Surface")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar BasePaintGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialGlossinessMapFeature"/> is invert.
        /// </summary>
        /// <value><c>true</c> if invert; otherwise, <c>false</c>.</value>
        /// <userdoc>When checked, considers the map as a roughness map instead of a glossiness map. 
        /// A roughness value of 1.0 corresponds to a glossiness value of 0.0 and vice-versa.</userdoc>
        [Display("Invert", "Micro Surface")]
        [DefaultValue(false)]
        public bool Invert { get; set; }

        /// <summary>
        /// Gets or sets the clear coat metalness map.
        /// </summary>
        /// <value>The metalness map.</value>
        [Display("Clear Coat Metalness Map", "Specular")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatMetalnessMap { get; set; }

        public MaterialClearCoatFeature()
        {
            MetalFlakesDiffuseMap = new ComputeColor(new Color4(Color.Black));
        }

        public override void MultipassGeneration(MaterialGeneratorContext context)
        {
            const int passCount = 2;

            context.SetMultiplePasses("ClearCoat", passCount);
        }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            if (!Enabled)
                return;
            
            ClampInputs();

            context.MaterialPass.BlendState = BlendStates.Additive;

            var isMetalFlakesPass = context.PassIndex == 0;

            if (isMetalFlakesPass)
            {
                // Diffuse Feature
                var metalFlakesComputeColorSource = MetalFlakesDiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));

                var mixinDiffuse = new ShaderMixinSource();
                mixinDiffuse.Mixins.Add(new ShaderClassSource("MaterialSurfaceDiffuseMetalFlakes"));
                mixinDiffuse.AddComposition("metalFlakesDiffuseMap", metalFlakesComputeColorSource);

                context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.DiffuseStream.Stream);
                context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.ColorBaseStream.Stream);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixinDiffuse);

                // Glossiness Feature
                context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");
                var baseGlossinessComputeColorMap = BasePaintGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
                var mixinGlossiness = new ShaderMixinSource();
                mixinGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMapMetalFlakes", Invert));
                mixinGlossiness.AddComposition("baseGlossinessMap", baseGlossinessComputeColorMap);            
                context.AddShaderSource(MaterialShaderStage.Pixel, mixinGlossiness);
            }
            else
            {
                // Enable transparency for clear coat pass only
                context.MaterialPass.HasTransparency = true;

                // Orange Peel Normal Map
                var mixinNormalMap = new ShaderMixinSource();
                // Inform the context that we are using matNormal (from the MaterialSurfaceNormalMap shader)
                context.UseStreamWithCustomBlend(MaterialShaderStage.Pixel, "matNormal", new ShaderClassSource("MaterialStreamNormalBlend"));
                context.Parameters.Set(MaterialKeys.HasNormalMap, true);

                mixinNormalMap.Mixins.Add(new ShaderClassSource("MaterialSurfaceNormalMap", IsXYNormalOrangePeel, ScaleAndBiasOrangePeel));

                var computeColorKeys = new MaterialComputeColorKeys(MaterialKeys.NormalMap, MaterialKeys.NormalValue, MaterialNormalMapFeature.DefaultNormalColor, false);
                var computeColorSource = ClearCoatLayerNormalMap.GenerateShaderSource(context, computeColorKeys);

                mixinNormalMap.AddComposition("normalMap", computeColorSource);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixinNormalMap);

                // Glossiness Feature
                context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");
                var clearCoatGlossinessComputeColorMap = ClearCoatGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
                var mixinGlossiness = new ShaderMixinSource();
                mixinGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMap", Invert));
                mixinGlossiness.AddComposition("glossinessMap", clearCoatGlossinessComputeColorMap);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixinGlossiness);

                // Metalness Feature
                var clearCoatMetalness = ClearCoatMetalnessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.MetalnessMap, MaterialKeys.MetalnessValue));

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceMetalness"));
                mixin.AddComposition("metalnessMap", clearCoatMetalness);
                context.UseStream(MaterialShaderStage.Pixel, "matSpecular");
                context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
            }
        }
        
        private void ClampInputs()
        {
            // Clamp color inputs
            var diffuseMin = Vector4.Zero;
            var diffuseMax = Vector4.One;

            MetalFlakesDiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);

            // Clamp scalar inputs
            BasePaintGlossinessMap.ClampFloat(0, 1);
            ClearCoatGlossinessMap.ClampFloat(0, 1);

            ClearCoatMetalnessMap.ClampFloat(0, 1);
        }
    }
}
