// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// A transparent additive material.
    /// </summary>
    [DataContract("MaterialTransparencyAdditiveFeature")]
    [Display("Additive")]
    public class MaterialTransparencyAdditiveFeature : IMaterialTransparencyFeature
    {
        private static readonly MaterialStreamDescriptor AlphaBlendStream = new MaterialStreamDescriptor("Alpha", "matAlphaBlend", MaterialKeys.AlphaBlendValue.PropertyType);

        private static readonly MaterialStreamDescriptor AlphaBlendColorStream = new MaterialStreamDescriptor("Alpha - Color", "matAlphaBlendColor", MaterialKeys.AlphaBlendColorValue.PropertyType);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTransparencyAdditiveFeature"/> class.
        /// </summary>
        public MaterialTransparencyAdditiveFeature()
        {
            Alpha = new ComputeFloat(1.0f);
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

        /// <summary>
        /// Gets or sets a value indicating whether [apply to diffuse].
        /// </summary>
        /// <value><c>true</c> if [apply to diffuse]; otherwise, <c>false</c>.</value>
        [DataMember(30)]
        [DefaultValue(false)]
        public bool ApplyToDiffuse { get; set; }

        public void Visit(MaterialGeneratorContext context)
        {
            var alpha = Alpha ?? new ComputeFloat(1.0f);
            var tint = Tint ?? new ComputeColor(Color.White);

            var blendDesc = new BlendStateDescription(Blend.One, Blend.BlendFactor);
            context.Material.HasTransparency = true;
            context.Parameters.Set(Effect.BlendStateKey, BlendState.NewFake(blendDesc));

            var alphaColor = alpha.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.AlphaBlendMap, MaterialKeys.AlphaBlendValue, Color.White));

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("ComputeColorMaterialAlphaBlend", ApplyToDiffuse));
            mixin.AddComposition("color", alphaColor);

            context.SetStream(MaterialShaderStage.Pixel, AlphaBlendStream.Stream, MaterialStreamType.Float2, mixin);
            context.SetStream(AlphaBlendColorStream.Stream, tint, MaterialKeys.AlphaBlendColorMap, MaterialKeys.AlphaBlendColorValue, Color.White);
        }
    }
}