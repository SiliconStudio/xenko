// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Applies a McIntosh blur to a texture. (Hexagonal bokeh)
    /// </summary>
    /// <remarks>
    /// This is a 3-pass (+1 final gathering) technique based on the paper of McIntosh from the Simon Fraser University. (2012)
    /// http://ivizlab.sfu.ca/papers/cgf2012.pdf
    /// </remarks>
    public class McIntoshBokeh : BokehBlur
    {
        private ImageEffect directionalBlurEffect;
        private ImageEffect finalCombineEffect;
        private ImageEffect optimizedEffect;

        // Number of tap required along one direction. (Not the total number of tap.)
        private int tapCount;

        // Weight of each tap
        private float[] tapWeights;

        // Simple flag to switch between the debug version or the optimized version
        private bool useOptimizedPath = true;

        /// <summary>
        /// Phase of the bokeh effect. (rotation angle in radian)
        /// </summary>
        public float Phase { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="McIntoshBokeh"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public McIntoshBokeh(ImageEffectContext context)
            : base(context)
        {
            directionalBlurEffect = new ImageEffectShader(context, "DepthAwareUniformBlurEffect");
            finalCombineEffect = new ImageEffectShader(context, "McIntoshCombineShader");
            optimizedEffect = new ImageEffectShader(context, "McIntoshOptimizedEffect");
            Phase = 0f;
        }

        /// <inheritdoc/>
        public override void SetRadius(float value)
        {
            base.SetRadius(value);

            //Special case for McIntosh blur: we need to apply a radius double of the final result radius.
            radius *= 2f;
            // Our actual total number of tap
            tapCount = (int)radius + 1;
        }

        protected override void DrawCore()
        {
            // Make sure we keep our uniform weights in synchronization with the number of taps
            if (tapWeights == null || tapWeights.Length != tapCount)
            {
                tapWeights = DoFUtil.GetUniformWeightBlurArray(tapCount);
            }

            if (!useOptimizedPath)
            {
                DrawCoreNaive();
            }
            else
            {
                DrawCoreOptimized();
            }
        }

        // Naive approach: 4 passes. (Reference version)
        private void DrawCoreNaive()
        {

            var originalTexture = GetSafeInput(0);
            var originalDepthBuffer = GetSafeInput(1);
            var outputTexture = GetSafeOutput(0);

            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurKeys.Count, tapCount);
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Radius, radius);
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.TapWeights, tapWeights);
            var tapNumber = 2 * tapCount - 1;

            // Blur in one direction
            var blurAngle = Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var firstBlurTexture = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, originalTexture);
            directionalBlurEffect.SetInput(1, originalDepthBuffer);
            directionalBlurEffect.SetOutput(firstBlurTexture);
            directionalBlurEffect.Draw("McIntoshBokehPass1_tap{0}_radius{1}", tapNumber, (int)radius);

            // Diagonal blur A
            blurAngle = MathUtil.Pi / 3f + Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var diagonalBlurA = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, firstBlurTexture);
            directionalBlurEffect.SetInput(1, originalDepthBuffer);
            directionalBlurEffect.SetOutput(diagonalBlurA);
            directionalBlurEffect.Draw("McIntoshBokehPass2A_tap{0}_radius{1}", tapNumber, (int)radius);

            // Diagonal blur B
            blurAngle = -MathUtil.Pi / 3f + Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var diagonalBlurB = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, firstBlurTexture);
            directionalBlurEffect.SetInput(1, originalDepthBuffer);
            directionalBlurEffect.SetOutput(diagonalBlurB);
            directionalBlurEffect.Draw("McIntoshBokehPass2B_tap{0}_radius{1}", tapNumber, (int)radius);

            // Final pass outputting the min of A and B
            finalCombineEffect.SetInput(0, diagonalBlurA);
            finalCombineEffect.SetInput(1, diagonalBlurB);
            finalCombineEffect.SetOutput(outputTexture);
            finalCombineEffect.Draw("McIntoshBokehPassCombine");
        }

        // Optimized approach: 2 passes.
        private void DrawCoreOptimized()
        {
            var originalTexture = GetSafeInput(0);
            var originalDepthBuffer = GetSafeInput(1);
            var outputTexture = GetSafeOutput(0);

            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurKeys.Count, tapCount);
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Radius, radius);
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.TapWeights, tapWeights);

            var tapNumber = 2 * tapCount - 1;

            // Blur in one direction
            var blurAngle = Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var firstBlurTexture = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, originalTexture);
            directionalBlurEffect.SetInput(1, originalDepthBuffer);
            directionalBlurEffect.SetOutput(firstBlurTexture);
            directionalBlurEffect.Draw("McIntoshBokehPass1_tap{0}_radius{1}", tapNumber, (int)radius);

            // Calculates the 2 diagonal blurs and keep the min of them
            var diagonalBlurAngleA =  MathUtil.Pi / 3f + Phase;
            var diagonalBlurAngleB = -MathUtil.Pi / 3f + Phase;
            optimizedEffect.SetInput(0, firstBlurTexture);
            optimizedEffect.SetInput(1, originalDepthBuffer);
            optimizedEffect.SetOutput(outputTexture);
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurKeys.Count, tapCount);
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Radius.ComposeWith("directionalBlurA"), radius);
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Direction.ComposeWith("directionalBlurA"), new Vector2((float)Math.Cos(diagonalBlurAngleA), (float)Math.Sin(diagonalBlurAngleA)));
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.TapWeights.ComposeWith("directionalBlurA"), tapWeights);
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Radius.ComposeWith("directionalBlurB"), radius);
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.Direction.ComposeWith("directionalBlurB"), new Vector2((float)Math.Cos(diagonalBlurAngleB), (float)Math.Sin(diagonalBlurAngleB)));
            optimizedEffect.Parameters.Set(DepthAwareUniformBlurUtilKeys.TapWeights.ComposeWith("directionalBlurB"), tapWeights);
            optimizedEffect.Draw("McIntoshBokehPass2_BlurABCombine_tap{0}_radius{1}", tapNumber, (int)radius);
        }

    }
}