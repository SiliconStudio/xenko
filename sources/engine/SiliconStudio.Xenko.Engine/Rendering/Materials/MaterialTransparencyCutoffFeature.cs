// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A transparent cutoff material.
    /// </summary>
    [DataContract("MaterialTransparencyCutoffFeature")]
    [Display("Cutoff")]
    public class MaterialTransparencyCutoffFeature : MaterialFeature, IMaterialTransparencyFeature
    {
        private const float DefaultAlpha = 0.5f;

        private static readonly MaterialStreamDescriptor AlphaDiscardStream = new MaterialStreamDescriptor("Alpha Discard", "matAlphaDiscard", MaterialKeys.AlphaDiscardValue.PropertyType);

        private static readonly PropertyKey<bool> HasFinalCallback = new PropertyKey<bool>("MaterialTransparencyCutoffFeature.HasFinalCallback", typeof(MaterialTransparencyCutoffFeature));

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTransparencyCutoffFeature"/> class.
        /// </summary>
        public MaterialTransparencyCutoffFeature()
        {
            Alpha = new ComputeFloat(DefaultAlpha);
        }

        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>The alpha.</value>
        /// <userdoc>The alpha threshold of the cutoff. All alpha values above this threshold are considered as fully transparent.
        /// All alpha values under this threshold are considered as fully opaque.</userdoc>
        [NotNull]
        [DataMember(10)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public IComputeScalar Alpha { get; set; }

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            var alpha = Alpha ?? new ComputeFloat(DefaultAlpha);
            context.SetStream(AlphaDiscardStream.Stream, alpha, MaterialKeys.AlphaDiscardMap, MaterialKeys.AlphaDiscardValue, new Color(DefaultAlpha));

            if (!context.Tags.Get(HasFinalCallback))
            {
                context.Tags.Set(HasFinalCallback, true);
                context.AddFinalCallback(MaterialShaderStage.Pixel, AddDiscardFromLuminance);
            }
        }

        private void AddDiscardFromLuminance(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            context.AddShaderSource(MaterialShaderStage.Pixel, new ShaderClassSource("MaterialSurfaceTransparentAlphaDiscard"));
        }
    }
}