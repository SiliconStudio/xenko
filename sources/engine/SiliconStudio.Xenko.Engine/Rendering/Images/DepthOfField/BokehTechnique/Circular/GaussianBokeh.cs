// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Applies a depth-aware gaussian blur to a texture. 
    /// </summary>
    /// <remarks>
    /// This does not produce beautiful bokeh shapes, but it is quite light-weight 
    /// and performance-friendly.
    /// </remarks>
    public class GaussianBokeh : BokehBlur
    {
        private ImageEffect directionalBlurEffect;

        private int tapCount;

        private float[] tapWeights;

        private bool weightsDirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianBokeh"/> class.
        /// </summary>
        public GaussianBokeh()
        {
        }

        public override float Radius
        {
            get
            {
                return base.Radius;
            }
            set
            {
                float oldRadius = Radius;
                base.Radius = value;
                weightsDirty = (oldRadius != Radius);
            }
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            directionalBlurEffect = ToLoadAndUnload(new ImageEffectShader("DepthAwareDirectionalBlurEffect"));
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Update the weight array if necessary
            if (weightsDirty || tapCount == 0)
            {
                weightsDirty = false;
                Vector2[] gaussianWeights = GaussianUtil.Calculate1D((int)Radius, 2f, true);
                tapCount = gaussianWeights.Length;
                tapWeights = new float[tapCount];
                for (int i = 0; i < tapCount; i++)
                {
                    tapWeights[i] = gaussianWeights[i].Y;
                }
            }

            var originalTexture = GetSafeInput(0);
            var outputTexture = GetSafeOutput(0);

            var tapNumber = 2 * tapCount - 1;
            directionalBlurEffect.Parameters.SetValueSlow(DepthAwareDirectionalBlurKeys.Count, tapCount);
            directionalBlurEffect.Parameters.SetValueSlow(DepthAwareDirectionalBlurKeys.TotalTap, tapNumber);
            directionalBlurEffect.Parameters.SetValueSlow(DepthAwareDirectionalBlurUtilKeys.Radius, Radius);
            directionalBlurEffect.Parameters.SetValueSlow(DepthAwareDirectionalBlurUtilKeys.TapWeights, tapWeights);

            // Blur in one direction
            var blurAngle = 0f;
            directionalBlurEffect.Parameters.SetValueSlow(DepthAwareDirectionalBlurUtilKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            var firstBlurTexture = NewScopedRenderTarget2D(originalTexture.Description);
            directionalBlurEffect.SetInput(0, originalTexture);
            directionalBlurEffect.SetOutput(firstBlurTexture);
            directionalBlurEffect.Draw(context, "GaussianBokehPass1_tap{0}_radius{1}", tapNumber, (int)Radius);

            // Second blur pass to ouput the final result
            blurAngle = MathUtil.PiOverTwo;
            directionalBlurEffect.Parameters.SetValueSlow(DepthAwareDirectionalBlurUtilKeys.Direction, new Vector2((float)Math.Cos(blurAngle), (float)Math.Sin(blurAngle)));

            directionalBlurEffect.SetInput(0, firstBlurTexture);
            directionalBlurEffect.SetOutput(outputTexture);
            directionalBlurEffect.Draw(context, "GaussianBokehPass2_tap{0}_radius{1}", tapNumber, (int)Radius);
        }

    }
}
