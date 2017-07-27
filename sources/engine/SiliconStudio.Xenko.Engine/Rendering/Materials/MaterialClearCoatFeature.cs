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

    [CategoryOrder(5, "Base Paint")]
    [CategoryOrder(10, "Metal Flakes")]
    [CategoryOrder(15, "Clear Coat")]
    public class MaterialClearCoatFeature : MaterialFeature, IMaterialClearCoatFeature
    {
        /// <summary>
        /// Gets or sets the distance at which the base paint layer should transition to the metal flakes layer.
        /// </summary>
        /// <value>The transition distance.</value>
        /// <userdoc>
        /// The transition distance value.
        /// </userdoc>
        [DataMember(100)]
        [Display("Layer Transition Distance")]
        [NotNull]
        [DataMemberRange(0.001, 2.000, 0.0100, 0.100, 3)]
        public IComputeScalar LODDistance { get; set; }

        #region Base Paint Layer Parameters
        /// <summary>
        /// Gets or sets the base paint layer diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        /// <userdoc>
        /// The diffuse map used by the base paint layer.
        /// </userdoc>
        [DataMember(110)]
        [Display("Base Paint Diffuse Map", "Base Paint")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor BasePaintDiffuseMap { get; set; }
        
        /// <summary>
        /// Gets or sets the base paint smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        /// <userdoc>
        /// The glossiness map used by the base paint layer.
        /// </userdoc>
        [DataMember(120)]
        [Display("Base Paint Glossiness Map", "Base Paint")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar BasePaintGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="BasePaintGlossinessMap"/> is invert.
        /// </summary>
        /// <value><c>true</c> if invert; otherwise, <c>false</c>.</value>
        /// <userdoc>When checked, considers the map as a roughness map instead of a glossiness map. 
        /// A roughness value of 1.0 corresponds to a glossiness value of 0.0 and vice-versa.</userdoc>
        [DataMember(130)]
        [Display("Invert Glossiness", "Base Paint")]
        [DefaultValue(false)]
        public bool BasePaintGlossinessInvert { get; set; }
        #endregion

        #region Metal Flakes Layer Parameters
        /// <summary>
        /// Gets or sets the metal flakes diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        /// <userdoc>
        /// The diffuse map used by the metal flakes layer.
        /// </userdoc>
        [DataMember(140)]
        [Display("Metal Flakes Diffuse Map", "Metal Flakes")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor MetalFlakesDiffuseMap { get; set; }

        /// <summary>
        /// Gets or sets the metal flakes smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        /// <userdoc>
        /// The glossiness map used by the metal flakes layer.
        /// </userdoc>
        [DataMember(150)]
        [Display("Metal Flakes Glossiness Map", "Metal Flakes")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar MetalFlakesGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MetalFlakesGlossinessMap"/> is invert.
        /// </summary>
        /// <value><c>true</c> if invert; otherwise, <c>false</c>.</value>
        /// <userdoc>When checked, considers the map as a roughness map instead of a glossiness map. 
        /// A roughness value of 1.0 corresponds to a glossiness value of 0.0 and vice-versa.</userdoc>
        [DataMember(160)]
        [Display("Invert Glossiness", "Metal Flakes")]
        [DefaultValue(false)]
        public bool MetalFlakesGlossinessInvert { get; set; } = false;

        /// <summary>
        /// Gets or sets the metal flakes metalness map.
        /// </summary>
        /// <userdoc>
        /// The metalness map used by the metal flakes layer.
        /// </userdoc>
        [DataMember(170)]
        [Display("Metal Flakes Metalness Map", "Metal Flakes")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar MetalFlakesMetalnessMap { get; set; }

        /// <summary>
        /// Gets or sets the normal map used for the metal flakes layer.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map used by the metal flakes layer.
        /// </userdoc>
        [DataMember(180)]
        [Display("Metal Flakes Normal Map", "Metal Flakes")]
        [NotNull]
        public IComputeColor MetalFlakesNormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2) and offset by (-1,-1) the normal map.
        /// </summary>
        /// <value><c>true</c> if scale and offset this normal map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale the XY by (2,2) and offset by (-1,-1). Required to unpack unsigned values of [0..1] to signed coordinates of [-1..+1].
        /// </userdoc>
        [DataMember(190)]
        [DefaultValue(true)]
        [Display("Scale & Offset", "Metal Flakes")]
        public bool MetalFlakesScaleAndBias { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be sqrt(1 - x*x - y*y).
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// The Z component of the normal vector will be calculated from X and Y assuming Z = sqrt(1 - x*x - y*y).
        /// </userdoc>
        [DataMember(200)]
        [DefaultValue(false)]
        [Display("Reconstruct Z", "Metal Flakes")]
        public bool MetalFlakeslIsXYNormal { get; set; }
        #endregion

        #region Clear Coat Layer Parameters
        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        /// <userdoc>
        /// The glossiness map used by the clear coat layer.
        /// </userdoc>
        [DataMember(210)]
        [Display("Clear Coat Glossiness Map", "Clear Coat")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ClearCoatGlossinessMap"/> is invert.
        /// </summary>
        /// <value><c>true</c> if invert; otherwise, <c>false</c>.</value>
        /// <userdoc>When checked, considers the map as a roughness map instead of a glossiness map. 
        /// A roughness value of 1.0 corresponds to a glossiness value of 0.0 and vice-versa.</userdoc>
        [DataMember(220)]
        [Display("Invert Glossiness", "Clear Coat")]
        [DefaultValue(false)]
        public bool ClearCoatGlossinessInvert { get; set; }

        /// <summary>
        /// Gets or sets the clear coat metalness map.
        /// </summary>
        /// <userdoc>
        /// The metalness map used by the clear coat layer.
        /// </userdoc>
        [DataMember(230)]
        [Display("Clear Coat Metalness Map", "Clear Coat")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatMetalnessMap { get; set; }

        /// <summary>
        /// Gets or sets the normal map used for the clear coat layer.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map used by the clear coat layer.
        /// </userdoc>
        [DataMember(240)]
        [Display("Orange Peel Normal Map", "Clear Coat")]
        [NotNull]
        public IComputeColor OrangePeelNormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2) and offset by (-1,-1) the normal map.
        /// </summary>
        /// <value><c>true</c> if scale and offset this normal map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale the XY by (2,2) and offset by (-1,-1). Required to unpack unsigned values of [0..1] to signed coordinates of [-1..+1].
        /// </userdoc>
        [DataMember(250)]
        [DefaultValue(true)]
        [Display("Scale & Offset", "Clear Coat")]
        public bool OrangePeelScaleAndBias { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be sqrt(1 - x*x - y*y).
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// The Z component of the normal vector will be calculated from X and Y assuming Z = sqrt(1 - x*x - y*y).
        /// </userdoc>
        [DataMember(260)]
        [DefaultValue(false)]
        [Display("Reconstruct Z", "Clear Coat")]
        public bool OrangePeelIsXYNormal { get; set; }
        #endregion

        public MaterialClearCoatFeature()
        {
            BasePaintGlossinessMap = new ComputeFloat();
            BasePaintDiffuseMap = new ComputeColor();

            MetalFlakesDiffuseMap = new ComputeColor();
            MetalFlakesNormalMap = new ComputeColor();
            MetalFlakesGlossinessMap = new ComputeFloat();
            MetalFlakesMetalnessMap = new ComputeFloat();

            OrangePeelNormalMap = new ComputeColor();
            ClearCoatGlossinessMap = new ComputeFloat();
            ClearCoatMetalnessMap = new ComputeFloat();

            LODDistance = new ComputeFloat(1.000f);
        }

        public override void MultipassGeneration(MaterialGeneratorContext context)
        {
            const int passCount = 2;
            context.SetMultiplePasses("ClearCoat", passCount);
        }

        // TODO Quantify/clean all the functions
        private void AddBaseDiffuse(MaterialGeneratorContext context)
        {
            var computeColorDiffuse = BasePaintDiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
            var mixinBaseDiffuse = new ShaderMixinSource();
            mixinBaseDiffuse.Mixins.Add(new ShaderClassSource("MaterialSurfaceDiffuse"));
            mixinBaseDiffuse.AddComposition("diffuseMap", computeColorDiffuse);
            context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.DiffuseStream.Stream);
            context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.ColorBaseStream.Stream);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixinBaseDiffuse);
        }

        private void AddBaseGlossiness(MaterialGeneratorContext context)
        {
            // Glossiness Feature
            context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");
            var metalFlakesGlossinessComputeColorMap = MetalFlakesGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
            var mixinBaseGlossiness = new ShaderMixinSource();
            mixinBaseGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMap", MetalFlakesGlossinessInvert));
            mixinBaseGlossiness.AddComposition("glossinessMap", metalFlakesGlossinessComputeColorMap);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixinBaseGlossiness);
        }

        private void AddMetalFlakesDiffuse(MaterialGeneratorContext context)
        {
            var surfaceToEyeDistance = LODDistance.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue, Color.White));

            // Diffuse Feature (interpolated by the 'regular' diffuse map)
            var metalFlakesComputeColorSource = MetalFlakesDiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));

            var mixinDiffuse = new ShaderMixinSource();

            // Diffuse uses a custom shader (to perform the interpolation)
            mixinDiffuse.Mixins.Add(new ShaderClassSource("MaterialSurfaceDiffuseMetalFlakes"));

            mixinDiffuse.AddComposition("diffuseMap", metalFlakesComputeColorSource);
            mixinDiffuse.AddComposition("surfaceToEyeDistanceFactor", surfaceToEyeDistance);

            context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.DiffuseStream.Stream);
            context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.ColorBaseStream.Stream);

            context.AddShaderSource(MaterialShaderStage.Pixel, mixinDiffuse);
        }

        private void AddMetalFlakesMetalness(MaterialGeneratorContext context)
        {
            // Metalness Feature
            var metalFlakesMetalness = MetalFlakesMetalnessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.MetalnessMap, MaterialKeys.MetalnessValue));

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceMetalness"));
            mixin.AddComposition("metalnessMap", metalFlakesMetalness);
            context.UseStream(MaterialShaderStage.Pixel, "matSpecular");
            context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
        }

        private void AddMetalFlakesNormal(MaterialGeneratorContext context)
        {
            var computeColorKeys = new MaterialComputeColorKeys(MaterialKeys.NormalMap, MaterialKeys.NormalValue, MaterialNormalMapFeature.DefaultNormalColor, false);
            var computeColorSource = MetalFlakesNormalMap.GenerateShaderSource(context, computeColorKeys);

            // Metal Flakes Normal Map
            var mixinNormalMap = new ShaderMixinSource();

            // Inform the context that we are using matNormal (from the MaterialSurfaceNormalMap shader)
            context.UseStreamWithCustomBlend(MaterialShaderStage.Pixel, "matNormal", new ShaderClassSource("MaterialStreamNormalBlend"));
            context.Parameters.Set(MaterialKeys.HasNormalMap, true);

            mixinNormalMap.Mixins.Add(new ShaderClassSource("MaterialSurfaceNormalMap", OrangePeelIsXYNormal, OrangePeelScaleAndBias));

            mixinNormalMap.AddComposition("normalMap", computeColorSource);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixinNormalMap);
        }

        private void AddMetalFlakesGlossiness(MaterialGeneratorContext context)
        {
            var surfaceToEyeDistance = LODDistance.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue, Color.White));

            // Metal Flakes Glossiness Feature
            context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");

            var baseGlossinessComputeColorMap = BasePaintGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));

            var mixinGlossiness = new ShaderMixinSource();

            // Computes glossiness factor for the metal flakes layer (based on the eye to surface distance and the base glossiness value)
            mixinGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMapMetalFlakes", BasePaintGlossinessInvert));

            mixinGlossiness.AddComposition("glossinessMap", baseGlossinessComputeColorMap);
            mixinGlossiness.AddComposition("surfaceToEyeDistanceFactor", surfaceToEyeDistance);

            context.AddShaderSource(MaterialShaderStage.Pixel, mixinGlossiness);
        }

        private void AddClearCoatNormalMap(MaterialGeneratorContext context)
        {
            var computeColorKeys = new MaterialComputeColorKeys(MaterialKeys.NormalMap, MaterialKeys.NormalValue, MaterialNormalMapFeature.DefaultNormalColor, false);
            var computeColorSource = OrangePeelNormalMap.GenerateShaderSource(context, computeColorKeys);

            // Orange Peel Normal Map
            var mixinNormalMap = new ShaderMixinSource();

            // Inform the context that we are using matNormal (from the MaterialSurfaceNormalMap shader)
            context.UseStreamWithCustomBlend(MaterialShaderStage.Pixel, "matNormal", new ShaderClassSource("MaterialStreamNormalBlend"));
            context.Parameters.Set(MaterialKeys.HasNormalMap, true);

            mixinNormalMap.Mixins.Add(new ShaderClassSource("MaterialSurfaceNormalMap", OrangePeelIsXYNormal, OrangePeelScaleAndBias));

            mixinNormalMap.AddComposition("normalMap", computeColorSource);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixinNormalMap);
        }

        private void AddClearCoatGlossinessMap(MaterialGeneratorContext context)
        {
            // Glossiness Feature
            context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");
            var clearCoatGlossinessComputeColorMap = ClearCoatGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
            var mixinGlossiness = new ShaderMixinSource();
            mixinGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMap", ClearCoatGlossinessInvert));
            mixinGlossiness.AddComposition("glossinessMap", clearCoatGlossinessComputeColorMap);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixinGlossiness);
        }

        private void AddClearCoatMetalnessMap(MaterialGeneratorContext context)
        {
            // Metalness Feature
            var clearCoatMetalness = ClearCoatMetalnessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.MetalnessMap, MaterialKeys.MetalnessValue));

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceMetalness"));
            mixin.AddComposition("metalnessMap", clearCoatMetalness);
            context.UseStream(MaterialShaderStage.Pixel, "matSpecular");
            context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
        }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            // Make sure the parameters are not out of range
            ClampInputs();

            // Set the blend state for both pass
            context.MaterialPass.BlendState = BlendStates.Additive;

            var isMetalFlakesPass = (context.PassIndex == 0);
            if (isMetalFlakesPass)
            {
                // Do the Base Paint first
                AddBaseDiffuse(context);
                AddBaseGlossiness(context);

                // Then the Metal Flakes
                AddMetalFlakesDiffuse(context);
                AddMetalFlakesNormal(context);
                AddMetalFlakesGlossiness(context);
                AddMetalFlakesMetalness(context);
            }
            else
            {
                // TODO Add reflections desaturation for environment reflections?
                // Ideally, this should be done on top of the regular specular model.
                // Unfortunately, after some tests, it seems that overriding the ComputeEnvironmentLightContribution is the only way to do so

                // Enable transparency for clear coat pass only
                context.MaterialPass.HasTransparency = true;

                AddClearCoatNormalMap(context);
                AddClearCoatGlossinessMap(context);
                AddClearCoatMetalnessMap(context);
            }
        }
        
        private void ClampInputs()
        {
            // Clamp color inputs
            var diffuseMin = Vector4.Zero;
            var diffuseMax = Vector4.One;

            BasePaintDiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);
            MetalFlakesDiffuseMap.ClampFloat4(ref diffuseMin, ref diffuseMax);
            
            // Clamp scalar inputs
            BasePaintGlossinessMap.ClampFloat(0, 1);
            MetalFlakesGlossinessMap.ClampFloat(0, 1);
            ClearCoatGlossinessMap.ClampFloat(0, 1);

            MetalFlakesMetalnessMap.ClampFloat(0, 1);
            ClearCoatMetalnessMap.ClampFloat(0, 1);
        }
    }
}
