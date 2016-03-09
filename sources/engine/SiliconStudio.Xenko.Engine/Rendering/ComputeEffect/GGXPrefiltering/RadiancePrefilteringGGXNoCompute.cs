// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.ComputeEffect.GGXPrefiltering
{
    /// <summary>
    /// A class for radiance pre-filtering using the GGX distribution function.
    /// </summary>
    public class RadiancePrefilteringGGXNoCompute : DrawEffect
    {
        private int samplingsCount;

        private readonly ImageEffectShader shader;

        /// <summary>
        /// Gets or sets the boolean indicating if the highest level of mipmaps should be let as-is or pre-filtered.
        /// </summary>
        public bool DoNotFilterHighestLevel { get; set; }

        /// <summary>
        /// Gets or sets the input radiance map to pre-filter.
        /// </summary>
        public Texture RadianceMap { get; set; }

        /// <summary>
        /// Gets or sets the texture to use to store the result of the pre-filtering.
        /// </summary>
        public Texture PrefilteredRadiance { get; set; }

        /// <summary>
        /// Gets or sets the number of pre-filtered mipmap to generate.
        /// </summary>
        public int MipmapGenerationCount { get; set; }

        /// <summary>
        /// Create a new instance of the class.
        /// </summary>
        /// <param name="context">the context</param>
        public RadiancePrefilteringGGXNoCompute(RenderContext context)
            : base(context, "RadiancePrefilteringGGX")
        {
            shader = new ImageEffectShader("RadiancePrefilteringGGXNoComputeEffect");
            DoNotFilterHighestLevel = true;
            samplingsCount = 1024;
        }

        /// <summary>
        /// Gets or sets the number of sampling used during the importance sampling
        /// </summary>
        /// <remarks>Should be a power of 2 and maximum value is 1024</remarks>
        public int SamplingsCount
        {
            get { return samplingsCount; }
            set
            {
                if (value > 1024)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (!MathUtil.IsPow2(value))
                    throw new ArgumentException("The provided value should be a power of 2");

                samplingsCount = Math.Max(1, value);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var output = PrefilteredRadiance;
            if (output == null || (output.Dimension != TextureDimension.Texture2D && output.Dimension != TextureDimension.TextureCube) || output.ArraySize != 6)
                throw new NotSupportedException("Only array of 2D textures are currently supported as output");

            if (!output.IsRenderTarget)
                throw new NotSupportedException("Only render targets are supported as output");

            var input = RadianceMap;
            if (input == null || input.Dimension != TextureDimension.TextureCube)
                throw new NotSupportedException("Only cubemaps are currently supported as input");

            var roughness = 0f;
            var faceCount = output.ArraySize;
            var levelSize = new Int2(output.Width, output.Height);
            var mipCount = MipmapGenerationCount == 0 ? output.MipLevels : MipmapGenerationCount;

            for (int mipLevel = 0; mipLevel < mipCount; mipLevel++)
            {
                if (mipLevel == 0 && DoNotFilterHighestLevel && input.Width >= output.Width)
                {
                    var inputLevel = MathUtil.Log2(input.Width / output.Width);
                    for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                    {
                        var inputSubresource = inputLevel + faceIndex * input.MipLevels;
                        var outputSubresource = 0 + faceIndex * output.MipLevels;
                        context.CommandList.CopyRegion(input, inputSubresource, null, output, outputSubresource);
                    }
                }
                else
                {
                    for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
                    {
                        using (var outputView = output.ToTextureView(ViewType.Single, faceIndex, mipLevel))
                        {
                            shader.Parameters.Set(RadiancePrefilteringGGXNoComputeShaderKeys.Face, faceIndex);
                            shader.Parameters.Set(RadiancePrefilteringGGXNoComputeShaderKeys.Roughness, roughness);
                            shader.Parameters.Set(RadiancePrefilteringGGXNoComputeShaderKeys.MipmapCount, input.MipLevels - 1);
                            shader.Parameters.Set(RadiancePrefilteringGGXNoComputeShaderKeys.RadianceMap, input);
                            shader.Parameters.Set(RadiancePrefilteringGGXNoComputeShaderKeys.RadianceMapSize, input.Width);
                            shader.Parameters.Set(RadiancePrefilteringGGXNoComputeParams.NbOfSamplings, SamplingsCount);
                            shader.SetOutput(outputView);
                            ((RendererBase)shader).Draw(context);
                        }
                    }
                }

                if (mipCount > 1)
                {
                    roughness += 1f / (mipCount - 1);
                    levelSize /= 2;
                }
            }
        }
    }
}