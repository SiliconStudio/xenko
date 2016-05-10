// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Materials.ComputeColors;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// The normal map for a surface material feature.
    /// </summary>
    [DataContract("MaterialNormalMapFeature")]
    [Display("Normal Map")]
    public class MaterialNormalMapFeature : MaterialFeature, IMaterialSurfaceFeature, IMaterialStreamProvider
    {
        private static readonly MaterialStreamDescriptor NormalStream = new MaterialStreamDescriptor("Normal", "matNormal", MaterialKeys.NormalValue.PropertyType);

        private static readonly Color DefaultNormalColor = new Color(0x80, 0x80, 0xFF, 0xFF);

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNormalMapFeature"/> class.
        /// </summary>
        public MaterialNormalMapFeature() : this(new ComputeTextureColor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialNormalMapFeature"/> class.
        /// </summary>
        /// <param name="normalMap">The normal map.</param>
        public MaterialNormalMapFeature(IComputeColor normalMap)
        {
            ScaleAndBias = true;
            NormalMap = normalMap;
        }

        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        /// <value>The normal map.</value>
        /// <userdoc>
        /// The normal map.
        /// </userdoc>
        [DataMember(10)]
        [Display("Normal Map")]
        [NotNull]
        public IComputeColor NormalMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale by (2,2,2) and bias by (-1,-1,-1) the normal map.
        /// </summary>
        /// <value><c>true</c> if scale and bias this normal map; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Scale by (2,2,2) and bias by (-1,-1,-1) this normal map.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(true)]
        [Display("Scale & Bias")]
        public bool ScaleAndBias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the normal is only stored in XY components and Z is assumed to be 1.0.
        /// </summary>
        /// <value><c>true</c> if this instance is xy normal; otherwise, <c>false</c>.</value>
        /// <userdoc>
        /// Read only xy components and assume z to be = 1. This is used for compressed normals.
        /// </userdoc>
        [DataMember(30)]
        [DefaultValue(false)]
        [Display("Normal xy")]
        public bool IsXYNormal { get; set; }

        public override void VisitFeature(MaterialGeneratorContext context)
        {
            if (NormalMap != null)
            {
                // Inform the context that we are using matNormal (from the MaterialSurfaceNormalMap shader)
                context.UseStreamWithCustomBlend(MaterialShaderStage.Pixel, NormalStream.Stream, new ShaderClassSource("MaterialStreamNormalBlend"));
                context.Parameters.Set(MaterialKeys.HasNormalMap, true);

                var normalMap = NormalMap;
                // Workaround to make sure that normal map are setup 
                var computeTextureColor = normalMap as ComputeTextureColor;
                if (computeTextureColor != null)
                {
                    if (computeTextureColor.FallbackValue.Value == Color.White)
                    {
                        computeTextureColor.FallbackValue.Value = DefaultNormalColor;
                    }
                }
                else
                {
                    var computeColor = normalMap as ComputeColor;
                    if (computeColor != null)
                    {
                        if (computeColor.Value == Color.Black || computeColor.Value == Color.White)
                        {
                            computeColor.Value = DefaultNormalColor;
                        }
                    }
                }

                var computeColorSource = NormalMap.GenerateShaderSource(context, new MaterialComputeColorKeys(MaterialKeys.NormalMap, MaterialKeys.NormalValue, DefaultNormalColor, false));
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("MaterialSurfaceNormalMap", IsXYNormal, ScaleAndBias));
                mixin.AddComposition("normalMap", computeColorSource);
                context.AddShaderSource(MaterialShaderStage.Pixel, mixin);
            }
        }

        public IEnumerable<MaterialStreamDescriptor> GetStreams()
        {
            yield return NormalStream;
        }
    }
}