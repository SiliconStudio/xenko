// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Provides a gaussian blur effect.
    /// </summary>
    /// <remarks>
    /// To improve performance of this gaussian blur is using:
    /// - a separable 1D horizontal and vertical blur
    /// - linear filtering to reduce the number of taps
    /// </remarks>
    public sealed class GaussianBlur : ImageEffect
    {
        private readonly ImageEffectShader blurH;
        private readonly ImageEffectShader blurV;

        private Vector2[] offsetsWeights;

        private int radius;

        private float sigmaRatio;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlur"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public GaussianBlur(ImageEffectContext context)
            : base(context)
        {
            // Use shared SharedParameters for blurH and blurV
            blurH = new ImageEffectShader(context, "GaussianBlurEffect", Parameters).DisposeBy(this);
            // Setup specific Horizontal parameter for blurH
            blurH.Parameters.Set(GaussianBlurKeys.VerticalBlur, false);

            blurV = new ImageEffectShader(context, "GaussianBlurEffect", Parameters).DisposeBy(this);
            // Setup specific Vertical parameter for blurV
            blurV.Parameters.Set(GaussianBlurKeys.VerticalBlur, true);

            Radius = 4;
            SigmaRatio = 2.0f;
        }

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        /// <value>The radius.</value>
        public int Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("Radius cannot be < 1");
                }

                if (radius != value)
                {
                    radius = value;
                    offsetsWeights = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the sigma ratio. The sigma ratio is used to calculate the sigma based on the radius: The actual
        /// formula is <c>sigma = radius / SigmaRatio</c>. The default value is 2.0f.
        /// </summary>
        /// <value>The sigma ratio.</value>
        public float SigmaRatio
        {
            get
            {
                return sigmaRatio;
            }
            set
            {
                if (value < 0.0f)
                {
                    throw new ArgumentOutOfRangeException("SigmaRatio cannot be < 0.0f");
                }

                if (sigmaRatio != value)
                {
                    sigmaRatio = value;
                    offsetsWeights = null;
                }
            }
        }

        protected override void DrawCore()
        {
            // Input texture
            var inputTexture = GetSafeInput(0);

            // Get a temporary texture for the intermediate pass
            // This texture will be allocated only in the scope of this draw and returned to the pool at the exit of this method
            var outputTextureH = NewScopedRenderTarget2D(inputTexture.Description);

            if (offsetsWeights == null)
            {
                // TODO: cache if necessary
                offsetsWeights = GaussianUtil.Calculate1D(Radius, SigmaRatio);
            }

            // Update shared parameters
            Parameters.Set(GaussianBlurKeys.Count, offsetsWeights.Length);
            Parameters.Set(GaussianBlurShaderKeys.OffsetsWeights, offsetsWeights);

            // Horizontal pass
            blurH.SetInput(inputTexture);
            blurH.SetOutput(outputTextureH);
            var size = Radius * 2 + 1;
            blurH.Draw("GaussianBlurH{0}x{0}", size);

            // Vertical pass
            blurV.SetInput(outputTextureH);
            blurV.SetOutput(GetSafeOutput(0));
            blurV.Draw("GaussianBlurV{0}x{0}", size);
        }
    }
}