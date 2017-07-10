// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    [DataContract("MaterialNormalMapCarPaint")]
    [Display("Car Paint Normal Map")]
    public class MaterialNormalMapCarPaint : MaterialNormalMapFeature
    {
        /// <summary>
        /// Gets or sets the normal map used for the clear coat layer.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map.
        /// </userdoc>
        [DataMember(50)]
        [Display("Orange Peel Normal Map")]
        [NotNull]
        public IComputeColor ClearCoatLayerNormalMap { get; set; }

        public override void MultipassGeneration(MaterialGeneratorContext context)
        {
            int passCount = 2;

            context.SetMultiplePasses("CarPaint", passCount);
        }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            IComputeColor temporaryMap = null;

            int passIndex = context.PassIndex % 2;
            
            if (passIndex == 1)
            {
                temporaryMap = NormalMap;
                NormalMap = ClearCoatLayerNormalMap;
            }

            base.GenerateShader(context);

            if (temporaryMap != null)
                NormalMap = temporaryMap;
            
            if (passIndex == 0)
            {
                context.MaterialPass.BlendState = BlendStates.Additive;
            }
            else if (passIndex == 1)
            {
                context.MaterialPass.BlendState = new BlendStateDescription(Blend.Zero, Blend.SourceColor) { RenderTarget0 = { AlphaSourceBlend = Blend.One, AlphaDestinationBlend = Blend.Zero } };             
            }
        }

        public bool Equals(MaterialSpecularThinGlassModelFeature other) => base.Equals(other);
    }
}
