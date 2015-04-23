// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// A transparent additive material.
    /// </summary>
    [DataContract("MaterialTransparencyAdditiveFeature")]
    [Display("Additive")]
    public class MaterialTransparencyAdditiveFeature : IMaterialTransparencyFeature
    {
        private static readonly MaterialStreamDescriptor AlphaBlendStream = new MaterialStreamDescriptor("DiffuseSpecularAlphaBlend", "matDiffuseSpecularAlphaBlend", MaterialKeys.DiffuseSpecularAlphaBlendValue.PropertyType);

        private static readonly MaterialStreamDescriptor AlphaBlendColorStream = new MaterialStreamDescriptor("DiffuseSpecularAlphaBlend - Color", "matAlphaBlendColor", MaterialKeys.AlphaBlendColorValue.PropertyType);

        private static readonly PropertyKey<bool> HasFinalCallback = new PropertyKey<bool>("MaterialTransparencyAdditiveFeature.HasFinalCallback", typeof(MaterialTransparencyAdditiveFeature));

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTransparencyAdditiveFeature"/> class.
        /// </summary>
        public MaterialTransparencyAdditiveFeature()
        {
            Alpha = new ComputeFloat(0.5f);
            Tint = new ComputeColor(Color.White);
        }

        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>The alpha.</value>
        [NotNull]
        [DataMember(10)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public IComputeScalar Alpha { get; set; }

        /// <summary>
        /// Gets or sets the tint color.
        /// </summary>
        /// <value>The tint.</value>
        /// <userdoc>A color to tint the transparency color</userdoc>
        [NotNull]
        [DataMember(20)]
        public IComputeColor Tint { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            var alpha = Alpha ?? new ComputeFloat(0.5f);
            var tint = Tint ?? new ComputeColor(Color.White);

            // Use pre-multiplied alpha to support both additive and alpha blending
            var blendDesc = new BlendStateDescription(Blend.One, Blend.InverseSourceAlpha);
            context.Material.HasTransparency = true;
            context.Parameters.Set(Effect.BlendStateKey, BlendState.NewFake(blendDesc));

            var alphaColor = alpha.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.DiffuseSpecularAlphaBlendMap, MaterialKeys.DiffuseSpecularAlphaBlendValue, Color.White));

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("ComputeColorMaterialAlphaBlend"));
            mixin.AddComposition("color", alphaColor);

            context.SetStream(MaterialShaderStage.Pixel, AlphaBlendStream.Stream, MaterialStreamType.Float2, mixin);
            context.SetStream(AlphaBlendColorStream.Stream, tint, MaterialKeys.AlphaBlendColorMap, MaterialKeys.AlphaBlendColorValue, Color.White);

            if (!context.Tags.Get(HasFinalCallback))
            {
                context.Tags.Set(HasFinalCallback, true);
                context.AddFinalCallback(MaterialShaderStage.Pixel, AddDiffuseSpecularAlphaBlendColor);
            }
        }

        private void AddDiffuseSpecularAlphaBlendColor(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            context.AddSurfaceShader(MaterialShaderStage.Pixel, new ShaderClassSource("MaterialSurfaceDiffuseSpecularAlphaBlendColor"));
        }
    }
}