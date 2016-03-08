// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Provides a gaussian blur effect.
    /// </summary>
    /// <remarks>
    /// To improve performance of this gaussian blur is using:
    /// - a separable 1D horizontal and vertical blur
    /// - linear filtering to reduce the number of taps
    /// </remarks>
    [DataContract("GaussianBlur")]
    [Display("Gaussian Blur")]
    public sealed class GaussianBlur : ImageEffect, IImageEffectRenderer // SceneEffectRenderer as GaussianBlur is a simple input/output effect.
    {
        private ImageEffectShader blurH;
        private ImageEffectShader blurV;
        private string nameGaussianBlurH;
        private string nameGaussianBlurV;

        private Vector2[] offsetsWeights;

        private int radius;

        private float sigmaRatio;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBlur"/> class.
        /// </summary>
        public GaussianBlur()
        {
            Radius = 4;
            SigmaRatio = 3.0f;
        }

        /// <inheritdoc/>
        protected override void InitializeCore()
        {
            base.InitializeCore();

            blurH = ToLoadAndUnload(new ImageEffectShader("GaussianBlurEffect"));
            blurV = ToLoadAndUnload(new ImageEffectShader("GaussianBlurEffect"));
            blurH.Initialize(Context);
            blurV.Initialize(Context);

            // Setup Horizontal parameters
            blurH.Parameters.Set(GaussianBlurKeys.VerticalBlur, false);
            blurV.Parameters.Set(GaussianBlurKeys.VerticalBlur, true);
        }

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        /// <value>The radius.</value>
        /// <userdoc>The radius of the Gaussian in pixels</userdoc>
        [DataMember(10)]
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
        /// <userdoc>The sigma ratio of the Gaussian. The sigma ratio is used to calculate the sigma of the Gaussian. 
        /// The actual formula is <c>sigma = radius / SigmaRatio</c></userdoc>
        [DataMember(20)]
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

        protected override void DrawCore(RenderDrawContext context)
        {
            // Input texture
            var inputTexture = GetSafeInput(0);

            // Get a temporary texture for the intermediate pass
            // This texture will be allocated only in the scope of this draw and returned to the pool at the exit of this method
            var desc = inputTexture.Description;
            desc.MultiSampleLevel = MSAALevel.None; // TODO we should have a method to get a non-MSAA RT
            var outputTextureH = NewScopedRenderTarget2D(desc);

            var size = Radius * 2 + 1;
            if (offsetsWeights == null)
            {
                nameGaussianBlurH = string.Format("GaussianBlurH{0}x{0}", size);
                nameGaussianBlurV = string.Format("GaussianBlurV{0}x{0}", size);
                
                // TODO: cache if necessary
                offsetsWeights = GaussianUtil.Calculate1D(Radius, SigmaRatio);
            }

            // Update permutation parameters
            blurH.Parameters.Set(GaussianBlurKeys.Count, offsetsWeights.Length);
            blurV.Parameters.Set(GaussianBlurKeys.Count, offsetsWeights.Length);
            blurH.EffectInstance.UpdateEffect(context.GraphicsDevice);
            blurV.EffectInstance.UpdateEffect(context.GraphicsDevice);

            // Update parameters
            blurH.Parameters.Set(GaussianBlurShaderKeys.OffsetsWeights, offsetsWeights);
            blurV.Parameters.Set(GaussianBlurShaderKeys.OffsetsWeights, offsetsWeights);

            // Horizontal pass
            blurH.SetInput(inputTexture);
            blurH.SetOutput(outputTextureH);
            blurH.Draw(context, nameGaussianBlurH);

            // Vertical pass
            blurV.SetInput(outputTextureH);
            blurV.SetOutput(GetSafeOutput(0));
            blurV.Draw(context, nameGaussianBlurV);
        }
    }
}