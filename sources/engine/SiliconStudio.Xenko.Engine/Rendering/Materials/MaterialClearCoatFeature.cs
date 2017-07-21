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
    public class MaterialClearCoatFeature : MaterialFeature, IMaterialClearCoatFeature
    {
        /// <summary>
        /// Gets or sets the distance factor to perform the transition between the metal flakes and base paint colors.
        /// </summary>
        [DataMember(110)]
        [Display("Paint Transition Distance")]
        [NotNull]
        [DataMemberRange(0.001, 2.000, 0.0100, 0.100, 3)]
        public IComputeScalar LODDistance { get; set; }

        /// <summary>
        /// Gets or sets the metal flakes diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        /// <userdoc>
        /// The diffuse map used by the metal flakes layer.
        /// </userdoc>
        [DataMember(120)]
        [Display("Base Paint Diffuse Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor BasePaintDiffuseMap { get; set; }

        /// <summary>
        /// Gets or sets the metal flakes diffuse map.
        /// </summary>
        /// <value>The diffuse map.</value>
        /// <userdoc>
        /// The diffuse map used by the metal flakes layer.
        /// </userdoc>
        [DataMember(120)]
        [Display("Metal Flakes Diffuse Map")]
        [NotNull]
        [DataMemberCustomSerializer]
        public IComputeColor MetalFlakesDiffuseMap { get; set; }

        /// <summary>
        /// Gets or sets the normal map used for the clear coat layer.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map used by the clear coat layer.
        /// </userdoc>
        [DataMember(130)]
        [Display("Metal Flakes Normal Map")]
        [NotNull]
        public IComputeColor MetalFlakesNormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2) and offset by (-1,-1) the normal map.
        /// </summary>
        /// <value><c>true</c> if scale and offset this normal map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale the XY by (2,2) and offset by (-1,-1). Required to unpack unsigned values of [0..1] to signed coordinates of [-1..+1].
        /// </userdoc>
        [DataMember(140)]
        [DefaultValue(true)]
        [Display("Scale & Offset")]
        public bool MetalFlakesScaleAndBias { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be sqrt(1 - x*x - y*y).
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// The Z component of the normal vector will be calculated from X and Y assuming Z = sqrt(1 - x*x - y*y).
        /// </userdoc>
        [DataMember(150)]
        [DefaultValue(false)]
        [Display("Reconstruct Z")]
        public bool MetalFlakeslIsXYNormal { get; set; }

        /// <summary>
        /// Gets or sets the normal map used for the clear coat layer.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map used by the clear coat layer.
        /// </userdoc>
        [DataMember(160)]
        [Display("Orange Peel Normal Map")]
        [NotNull]
        public IComputeColor OrangePeelNormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2) and offset by (-1,-1) the normal map.
        /// </summary>
        /// <value><c>true</c> if scale and offset this normal map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale the XY by (2,2) and offset by (-1,-1). Required to unpack unsigned values of [0..1] to signed coordinates of [-1..+1].
        /// </userdoc>
        [DataMember(170)]
        [DefaultValue(true)]
        [Display("Scale & Offset")]
        public bool OrangePeelScaleAndBias { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be sqrt(1 - x*x - y*y).
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// The Z component of the normal vector will be calculated from X and Y assuming Z = sqrt(1 - x*x - y*y).
        /// </userdoc>
        [DataMember(180)]
        [DefaultValue(false)]
        [Display("Reconstruct Z")]
        public bool OrangePeelIsXYNormal { get; set; }

        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        /// <userdoc>
        /// The glossiness map used by the clear coat layer.
        /// </userdoc>
        [DataMember(190)]
        [Display("Clear Coat Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets the base paint smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        /// <userdoc>
        /// The glossiness map used by the base paint layer.
        /// </userdoc>
        [DataMember(200)]
        [Display("Base Paint Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar BasePaintGlossinessMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialGlossinessMapFeature"/> is invert.
        /// </summary>
        /// <value><c>true</c> if invert; otherwise, <c>false</c>.</value>
        /// <userdoc>When checked, considers the map as a roughness map instead of a glossiness map. 
        /// A roughness value of 1.0 corresponds to a glossiness value of 0.0 and vice-versa.</userdoc>
        [DataMember(210)]
        [Display("Invert Glossiness")]
        [DefaultValue(false)]
        public bool Invert { get; set; }

        /// <summary>
        /// Gets or sets the clear coat metalness map.
        /// </summary>
        /// <userdoc>
        /// The metalness map used by the clear coat layer.
        /// </userdoc>
        [DataMember(220)]
        [Display("Clear Coat Metalness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar ClearCoatMetalnessMap { get; set; }

        /// <summary>
        /// Gets or sets the clear coat metalness map.
        /// </summary>
        /// <userdoc>
        /// The metalness map used by the clear coat layer.
        /// </userdoc>
        [DataMember(220)]
        [Display("Metal Flakes Metalness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar MetalFlakesMetalnessMap { get; set; }

        /// <summary>
        /// Gets or sets the clear coat smoothness map.
        /// </summary>
        /// <value>The smoothness map.</value>
        /// <userdoc>
        /// The glossiness map used by the clear coat layer.
        /// </userdoc>
        [DataMember(190)]
        [Display("Metal Flakes Glossiness Map")]
        [NotNull]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 3)]
        public IComputeScalar MetalFlakesGlossinessMap { get; set; }

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

            LODDistance = new ComputeFloat(2.000f);
        }

        public override void MultipassGeneration(MaterialGeneratorContext context)
        {
            const int passCount = 2;
            context.SetMultiplePasses("ClearCoat", passCount);
        }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            // Make sure the inputs are not out of range
            ClampInputs();

            // Set the blend state for both pass
            context.MaterialPass.BlendState = BlendStates.Additive;

            var isMetalFlakesPass = context.PassIndex == 0;
            if (isMetalFlakesPass)
            {
                var surfaceToEyeDistance = LODDistance.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue, Color.White));

                var computeColorDiffuse = BasePaintDiffuseMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseMap, MaterialKeys.DiffuseValue, Color.White));
                var mixinBaseDiffuse = new ShaderMixinSource();
                mixinBaseDiffuse.Mixins.Add(new ShaderClassSource("MaterialSurfaceDiffuse"));
                mixinBaseDiffuse.AddComposition("diffuseMap", computeColorDiffuse);
                context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.DiffuseStream.Stream);
                context.UseStream(MaterialShaderStage.Pixel, MaterialDiffuseMapFeature.ColorBaseStream.Stream);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixinBaseDiffuse);
                
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

                // Glossiness Feature
                context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");
                var metalFlakesGlossinessComputeColorMap = MetalFlakesGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));
                var mixinBaseGlossiness = new ShaderMixinSource();
                mixinBaseGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMap", Invert));
                mixinBaseGlossiness.AddComposition("glossinessMap", metalFlakesGlossinessComputeColorMap);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixinBaseGlossiness);

                // Metal Flakes Glossiness Feature
                context.UseStream(MaterialShaderStage.Pixel, "matGlossiness");

                var baseGlossinessComputeColorMap = BasePaintGlossinessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.GlossinessMap, MaterialKeys.GlossinessValue));

                var mixinGlossiness = new ShaderMixinSource();

                // Metalness Feature
                var metalFlakesMetalness = MetalFlakesMetalnessMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.MetalnessMap, MaterialKeys.MetalnessValue));

                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceMetalness"));
                mixin.AddComposition("metalnessMap", metalFlakesMetalness);
                context.UseStream(MaterialShaderStage.Pixel, "matSpecular");
                context.AddShaderSource(MaterialShaderStage.Pixel, mixin);

                // Computes glossiness factor for the metal flakes layer (based on the eye to surface distance and the base glossiness value)
                mixinGlossiness.Mixins.Add(new ShaderClassSource("MaterialSurfaceGlossinessMapMetalFlakes", Invert));

                mixinGlossiness.AddComposition("glossinessMap", baseGlossinessComputeColorMap);
                mixinGlossiness.AddComposition("surfaceToEyeDistanceFactor", surfaceToEyeDistance);

                context.AddShaderSource(MaterialShaderStage.Pixel, mixinGlossiness);
            }
            else
            {
                // TODO Add reflections desaturation for environment reflections?
                // Ideally, this should be done on top of the regular specular model.
                // Unfortunately, after some tests, it seems that overriding the ComputeEnvironmentLightContribution is the only way to do so

                // Enable transparency for clear coat pass only
                context.MaterialPass.HasTransparency = true;

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
