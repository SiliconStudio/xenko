// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
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
        [DataMember(110)]
        [Display("Orange Peel Normal Map")]
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
        [Display("Scale & Offset")]
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
        [Display("Reconstruct Z")]
        public bool IsXYNormalOrangePeel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNormalMapCarPaint"/> class.
        /// </summary>
        public MaterialNormalMapCarPaint() : base(new ComputeTextureColor())
        {
            ClearCoatLayerNormalMap = new ComputeTextureColor();

            ScaleAndBiasOrangePeel = ScaleAndBias = true;

            // Load default resources
            NormalMap = new ComputeTextureColor
            {
                Texture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("7e2761d1-ef86-420a-b7a7-a0ed1c16f9bb"), "XenkoCarPaintMetalFlakesNM"),
                Scale = new Vector2(128, 128),
                UseRandomTexCoordinates = true
            };

            ClearCoatLayerNormalMap = new ComputeTextureColor
            {
                Texture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("2f76bcba-ae9f-4954-b98d-f94c2102ff86"), "XenkoCarPaintOrangePeelNM"),
                Scale = new Vector2(8, 8)
            };
        }

        public override void GenerateShader(MaterialGeneratorContext context)
        {
            var passIndex = context.PassIndex;
            
            // TODO Move pass parameters somewhere else? (shared parameters for each layer rendering feature)
            context.MaterialPass.BlendState = BlendStates.Additive;
            // Enable transparency for clear coat pass only
            if (passIndex == 1)
                context.MaterialPass.HasTransparency = true;

            context.UseStreamWithCustomBlend(MaterialShaderStage.Pixel, NormalStream.Stream, new ShaderClassSource("MaterialStreamNormalBlend"));          
            context.Parameters.Set(MaterialKeys.HasNormalMap, true);
            var computeColorKeys = new MaterialComputeColorKeys(MaterialKeys.NormalMap, MaterialKeys.NormalValue, DefaultNormalColor, false);
            var computeColorSource = ((passIndex == 0) ? NormalMap : ClearCoatLayerNormalMap).GenerateShaderSource(context, computeColorKeys);

            var mixin = new ShaderMixinSource();
            mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceNormalMap", (passIndex == 0) ? IsXYNormal : IsXYNormalOrangePeel, (passIndex == 0) ? ScaleAndBias : ScaleAndBiasOrangePeel));
            mixin.AddComposition("normalMap", computeColorSource);
            context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
        }
        
        public bool Equals(MaterialNormalMapCarPaint other) => base.Equals(other);
    }
}
