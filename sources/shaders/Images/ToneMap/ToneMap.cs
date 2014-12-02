// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A tonemap effect.
    /// </summary>
    public class ToneMap : ImageEffectBase
    {
        private ParameterCollection[] sharedParameters;
        private readonly LuminanceEffect luminanceEffect;
        private readonly ImageEffect toneMap;

        private readonly float[] weightedLuminances = new float[16];
        private int currentWeightedLuminanceIndex = 0;
        private float previousLuminance;


        private readonly ParameterCollection parameters;

        private readonly ToneMapU2FilmicOperator defaultOperator;

        private readonly Stopwatch timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMap" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="toneMapEffect">The tone map shader effect (default is <c>ToneMapEffect)</c>.</param>
        /// <exception cref="System.ArgumentNullException">toneMapEffect</exception>
        public ToneMap(ImageEffectContext context, string toneMapEffect = "ToneMapEffect") : base(context)
        {
            if (toneMapEffect == null) throw new ArgumentNullException("toneMapEffect");
            parameters = new ParameterCollection();
            sharedParameters = new []
            {
                parameters, 
                null // Placeholder for Operator.Parameters
            };
            timer = new Stopwatch();
            luminanceEffect = new LuminanceEffect(context).DisposeBy(this);
            toneMap = new ImageEffect(context, toneMapEffect, sharedParameters).DisposeBy(this);
            defaultOperator = new ToneMapU2FilmicOperator();
            AdaptationRate = 1.25f;
        }

        /// <summary>
        /// Gets or sets the operator used for tonemap.
        /// </summary>
        /// <value>The operator.</value>
        public ToneMapOperator Operator { get; set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

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

        protected override void DrawCore()
        {
            var inputTexture = GetSafeInput(0);
            var outputTexture = GetSafeOutput(0);

            // ----------------------------
            // Luminance Pass
            // ----------------------------
            var lumSize = inputTexture.Size.Down2(2);
            var luminanceMap = NewScopedRenderTarget2D(lumSize.Width, lumSize.Height, PixelFormat.R16_Float);

            const int minSize = 8;
            var nextSize = lumSize;
            var upscaleCount = 0;
            while (nextSize.Width > minSize && nextSize.Height > minSize)
            {
                nextSize = nextSize.Down2();
                upscaleCount++;
            }

            // Perform a luminance pass
            luminanceEffect.UpscaleCount = upscaleCount;
            luminanceEffect.SetInput(inputTexture);
            luminanceEffect.SetOutput(luminanceMap);
            luminanceEffect.Draw("Luminance");

            // Update the luminance
            UpdateAverageLuminanceLog();

            // Update operator parameters
            var currentOperator = Operator ?? defaultOperator;
            currentOperator.UpdateParameters();

            // Use operator parameters for shared parameters
            sharedParameters[1] = currentOperator.Parameters;

            // Run the tonemap
            toneMap.SetInput(inputTexture, luminanceMap);
            toneMap.SetOutput(outputTexture);
            toneMap.Draw();
        }

        private void UpdateAverageLuminanceLog()
        {
            var elapsedTime = timer.Elapsed;
            timer.Restart();

            // Adapt the luminance using Pattanaik's technique    
            var currentAvgLuminance = (float)Math.Max(luminanceEffect.AverageLuminance, 0.0001);
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
            var avgLuminanceLog = (float)Math.Log(adaptedLum, 2);
            previousLuminance = adaptedLum;
            //Trace.WriteLine(string.Format("Adapted: {0} Luminance: {1}", adaptedLum, currentAvgLuminance));

            if (AutoKeyValue)
            {
                KeyValue = 1.03f - (2.0f / (2.0f + (float)Math.Log10(adaptedLum + 1)));
            }

            // Setup parameters
            Parameters.Set(ToneMapShaderKeys.LuminanceAverageGlobal, avgLuminanceLog);
        }
    }
}