// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A tonemap effect.
    /// </summary>
    public class ToneMap : ColorTransform
    {
        private readonly float[] weightedLuminances = new float[16];
        private int currentWeightedLuminanceIndex = 0;
        private float previousLuminance;
        private readonly ToneMapU2FilmicOperator defaultOperator;

        private readonly Stopwatch timer;

        private string previousShader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMap" /> class.
        /// </summary>
        /// <param name="toneMapEffect">The tone map shader effect (default is <c>ToneMapEffect)</c>.</param>
        /// <exception cref="System.ArgumentNullException">toneMapEffect</exception>
        public ToneMap(string toneMapEffect = "ToneMapEffect") : base(toneMapEffect)
        {
            timer = new Stopwatch();
            defaultOperator = new ToneMapU2FilmicOperator();
            AdaptationRate = 1.25f;
        }

        /// <summary>
        /// Gets or sets the operator used for tonemap.
        /// </summary>
        /// <value>The operator.</value>
        public ToneMapOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the key value.
        /// </summary>
        /// <value>The key value.</value>
        public float KeyValue
        {
            get
            {
                return Parameters.Get(ToneMapShaderKeys.KeyValue);
            }
            set
            {
                Parameters.Set(ToneMapShaderKeys.KeyValue, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [automatic key value].
        /// </summary>
        /// <value><c>true</c> if [automatic key value]; otherwise, <c>false</c>.</value>
        public bool AutoKeyValue
        {
            get
            {
                return Parameters.Get(ToneMapShaderKeys.AutoKeyValue);
            }
            set
            {
                Parameters.Set(ToneMapShaderKeys.AutoKeyValue, value);
            }
        }

        /// <summary>
        /// Gets or sets the adaptation rate.
        /// </summary>
        /// <value>The adaptation rate.</value>
        public float AdaptationRate { get; set; }

        /// <summary>
        /// Gets or sets the luminance local factor.
        /// </summary>
        /// <value>The luminance local factor.</value>
        public float LuminanceLocalFactor
        {
            get
            {
                return Parameters.Get(ToneMapShaderKeys.LuminanceLocalFactor);
            }
            set
            {
                Parameters.Set(ToneMapShaderKeys.LuminanceLocalFactor, value);
            }
        }

        /// <summary>
        /// Gets or sets the gamma.
        /// </summary>
        /// <value>The gamma.</value>
        public float Gamma
        {
            get
            {
                return Parameters.Get(ToneMapShaderKeys.Gamma);
            }
            set
            {
                Parameters.Set(ToneMapShaderKeys.Gamma, value);
            }
        }

        /// <summary>
        /// Gets or sets the contrast.
        /// </summary>
        /// <value>The contrast.</value>
        public float Contrast
        {
            get
            {
                return Parameters.Get(ToneMapShaderKeys.Contrast);
            }
            set
            {
                Parameters.Set(ToneMapShaderKeys.Contrast, value);
            }
        }

        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        /// <value>The brightness.</value>
        public float Brightness
        {
            get
            {
                return Parameters.Get(ToneMapShaderKeys.Brightness);
            }
            set
            {
                Parameters.Set(ToneMapShaderKeys.Brightness, value);
            }
        }

        public override void UpdateParameters(ColorTransformContext context)
        {
            base.UpdateParameters(context);

            // Update the luminance
            var elapsedTime = timer.Elapsed;
            timer.Restart();

            var luminanceResult = context.SharedParameters.Get(LuminanceEffect.LuminanceResult);

            var avgLuminanceLog = 0.18f; // TODO: Add a parmetrized average luminance
            if (luminanceResult.LocalTexture != null)
            {
                // Adapt the luminance using Pattanaik's technique    
                var currentAvgLuminance = (float)Math.Max(luminanceResult.AverageLuminance, 0.0001);
                weightedLuminances[currentWeightedLuminanceIndex] = currentAvgLuminance;
                currentWeightedLuminanceIndex = (currentWeightedLuminanceIndex + 1) % weightedLuminances.Length;

                float avgLuminannce = 0.0f;
                for (int i = 0; i < weightedLuminances.Length; i++)
                {
                    avgLuminannce += weightedLuminances[i];
                }
                avgLuminannce /= weightedLuminances.Length;

                // Get current avg luminance

                // Get adapted luminance
                var adaptedLum = (float)(previousLuminance + (avgLuminannce - previousLuminance) * (1.0 - Math.Exp(-elapsedTime.TotalSeconds * AdaptationRate)));
                avgLuminanceLog = (float)Math.Log(adaptedLum, 2);
                previousLuminance = adaptedLum;
                //Trace.WriteLine(string.Format("Adapted: {0} Luminance: {1}", adaptedLum, currentAvgLuminance));

                if (AutoKeyValue)
                {
                    KeyValue = 1.03f - (2.0f / (2.0f + (float)Math.Log10(adaptedLum + 1)));
                }
            }

            // Setup parameters
            Parameters.Set(ToneMapShaderKeys.LuminanceTexture, luminanceResult.LocalTexture);
            Parameters.Set(ToneMapShaderKeys.LuminanceAverageGlobal, avgLuminanceLog);

            // Update operator parameters
            var currentOperator = Operator ?? defaultOperator;
            currentOperator.UpdateParameters(context);

            // Copy sub parameters from composition to this transform
            foreach (var parameterValue in currentOperator.Parameters)
            {
                var key = parameterValue.Key.ComposeWith("ToneMapOperator");
                currentOperator.Parameters.CopySharedTo(parameterValue.Key, key, Parameters);
            }
        }
    }
}