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
    public class McIntoshBokeh : ImageEffect
    {
        private ImageEffect directionalBlurEffect;
        private ImageEffect finalCombineEffect;

        private float radius;

        private int tapCount;

        /// <summary>
        /// Phase of the bokeh effect. (rotation angle in radian)
        /// Note that only 0 or PI/2 give artifact-free results. 
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
            Radius = 5;
            Phase = 0f;
        }

        /// <summary>
        /// Gets or sets the radius.
        /// </summary>
        /// <value>The radius.</value>
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Radius cannot be < 0");
                }

                radius = value;
                //Tap number is directly linked to the radius.
                //We can't lower the count for higher performance because artifacts will appear immediately. 
                tapCount = (int)radius + 1;
            }
        }

        protected override void DrawCore()
        {
            var originalTexture = GetSafeInput(0);
            var originalDepthBuffer = GetSafeInput(1);
            var outputTexture = GetSafeOutput(0);

            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurKeys.Count, tapCount);
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurShaderKeys.Radius, radius);
            var tapNumber = 2 * tapCount - 1;

            // Blur in one direction
            var blurAngle = Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurShaderKeys.Direction, new Vector2( (float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var firstBlurTexture = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, originalTexture);
            directionalBlurEffect.SetInput(1, originalDepthBuffer);
            directionalBlurEffect.SetOutput(firstBlurTexture);
            directionalBlurEffect.Draw("McIntoshBokehPass1_tap{0}_radius{1}", tapNumber, (int)radius);

            // Diagonal blur A
            blurAngle = MathUtil.Pi / 3f + Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurShaderKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var diagonalBlurA = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, firstBlurTexture);
            directionalBlurEffect.SetOutput(diagonalBlurA);
            directionalBlurEffect.Draw("McIntoshBokehPass2A_tap{0}_radius{1}", tapNumber, (int)radius);

            // Diagonal blur B
            blurAngle = -MathUtil.Pi / 3f + Phase;
            directionalBlurEffect.Parameters.Set(DepthAwareUniformBlurShaderKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var diagonalBlurB = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, firstBlurTexture);
            directionalBlurEffect.SetOutput(diagonalBlurB);
            directionalBlurEffect.Draw("McIntoshBokehPass2B_tap{0}_radius{1}", tapNumber, (int)radius);

            // Final pass outputting the min of A and B
            finalCombineEffect.SetInput(0, diagonalBlurA);
            finalCombineEffect.SetInput(1, diagonalBlurB);
            finalCombineEffect.SetOutput(outputTexture);
            finalCombineEffect.Draw("McIntoshBokehPassCombine");
        }
    }
}