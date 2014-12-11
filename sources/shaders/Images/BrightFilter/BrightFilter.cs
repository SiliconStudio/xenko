// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A bright pass filter.
    /// </summary>
    public class BrightFilter : ImageEffectShader
    {
        public static readonly ParameterKey<Texture> TextureResult = ParameterKeys.New<Texture>();

        private Texture brightPassTexture;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrightFilter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brightPassShaderName">Name of the bright pass shader.</param>
        public BrightFilter(ImageEffectContext context, string brightPassShaderName = "BrightFilterShader")
            : base(context, brightPassShaderName)
        {
            DownscaleCount = 3;
        }

        /// <summary>
        /// Gets or sets the downscale count used when no output are given.
        /// </summary>
        /// <value>The downscale count.</value>
        public int DownscaleCount { get; set; }

        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        /// <value>The threshold.</value>
        public float Threshold
        {
            get
            {
                return Parameters.Get(BrightFilterShaderKeys.BrightPassThreshold);
            }
            set
            {
                Parameters.Set(BrightFilterShaderKeys.BrightPassThreshold, value);
            }
        }

        protected override void PreDrawCore(string name)
        {
            // If no output are setup on this effect, automatically create a downscale version of the input
            // And set the result back into the context via the key BrightFilter.BrightPassTexture
            var input = GetSafeInput(0);
            var outputTexture = GetOutput(0);
            if (outputTexture == null)
            {
                var outputSize = input.Size.Down2(DownscaleCount);
                brightPassTexture = Context.Allocator.GetTemporaryTexture2D(outputSize.Width, outputSize.Height, input.Format, 1);
                SetOutput(brightPassTexture);
            }
            else
            {
                Context.Allocator.ReleaseReference(brightPassTexture);
                brightPassTexture = null;
            }
            Context.Parameters.Set(TextureResult, brightPassTexture);

            base.PreDrawCore(name);
        }

        protected override void Destroy()
        {
            Context.Allocator.ReleaseReference(brightPassTexture);
            brightPassTexture = null;

            base.Destroy();
        }
    }
}