// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.ComputeEffect.GGXPrefiltering
{
    /// <summary>
    /// A class for radiance pre-filtering using the GGX distribution function.
    /// </summary>
    public class RadiancePrefilteringGGX : ComputeEffect
    {
        private int samplingsCount;

        private ComputeEffectShader computeShader;

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
        public RadiancePrefilteringGGX(DrawEffectContext context)
            : base(context, "RadiancePrefilteringGGX")
        {
            computeShader = new ComputeEffectShader(context) { ShaderSourceName = "RadiancePrefilteringGGXEffect" };
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
                    throw new ArgumentOutOfRangeException("value");

                if(!MathUtil.IsPow2(value))
                    throw new ArgumentException("The provided value should be a power of 2");

                samplingsCount = Math.Max(1, value);
            }
        }

        protected override void DrawCore()
        {
            base.DrawCore();

            var output = PrefilteredRadiance;
            if(output == null || output.Dimension != TextureDimension.Texture2D)
                throw new NotSupportedException("Only array of 2D textures are currently supported as output");

            var input = RadianceMap;
            if(input == null || input.Dimension != TextureDimension.TextureCube)
                throw new NotSupportedException("Only cubemaps are currently supported as input");

            if(DoNotFilterHighestLevel)
                throw new NotImplementedException();

            var roughness = 0f;
            var faceCount = output.ArraySize;
            var levelSize = new Int2(output.Width, output.Height);
            var startLevel = (int)Math.Max(0, Math.Round(Math.Log(input.Width / (float)output.Width)/Math.Log(2)));
            for (int l = 0; l < MipmapGenerationCount; l++)
            {
                var outputView = output.ToTextureView(ViewType.MipBand, 0, l);

                computeShader.ThreadGroupCounts = new Int3(levelSize.X, levelSize.Y, faceCount);
                computeShader.ThreadNumbers = new Int3(SamplingsCount, 1, 1);
                computeShader.Parameters.Set(RadiancePrefilteringGGXShaderKeys.Roughness, roughness);
                computeShader.Parameters.Set(RadiancePrefilteringGGXShaderKeys.MipmapLevel, startLevel+l);
                computeShader.Parameters.Set(RadiancePrefilteringGGXShaderKeys.RadianceMap, input);
                computeShader.Parameters.Set(RadiancePrefilteringGGXShaderKeys.FilteredRadiance, outputView);
                computeShader.Parameters.Set(RadiancePrefilteringGGXParams.NbOfSamplings, SamplingsCount);
                computeShader.Draw();

                outputView.Dispose();

                roughness += 1f / (MipmapGenerationCount-1);
                levelSize /= 2;
            }
        }
    }
}