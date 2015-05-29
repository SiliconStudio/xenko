// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// A tonemap effect.
    /// </summary>
    [DataContract("ToneMap")]
    public class ToneMap : ColorTransform
    {
        // Note: could be static if we use a lock
        private readonly Dictionary<ParameterKey, ParameterKey> tonemapKeys = new Dictionary<ParameterKey, ParameterKey>();
        private float previousLuminance;

        private readonly Stopwatch timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMap"/> class.
        /// </summary>
        public ToneMap() : this("ToneMapEffect")
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMap" /> class.
        /// </summary>
        /// <param name="toneMapEffect">The tone map shader effect (default is <c>ToneMapEffect)</c>.</param>
        /// <exception cref="System.ArgumentNullException">toneMapEffect</exception>
        public ToneMap(string toneMapEffect) : base(toneMapEffect)
        {
            timer = new Stopwatch();
            AutoKeyValue = true;
            Operator = new ToneMapHejlDawsonOperator();
            AdaptationRate = 1.0f;
        }

        /// <summary>
        /// Gets or sets the operator used for tonemap.
        /// </summary>
        /// <value>The operator.</value>
        [DataMember(10)]
        [NotNull]
        public ToneMapOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the key value.
        /// </summary>
        /// <value>The key value.</value>
        [DataMember(20)]
        [DefaultValue(0.18f)]
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
        [DataMember(30)]
        [DefaultValue(true)]
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
        [DataMember(40)]
        [DefaultValue(1.0f)]
        public float AdaptationRate { get; set; }

        /// <summary>
        /// Gets or sets the luminance local factor.
        /// </summary>
        /// <value>The luminance local factor.</value>
        [DataMember(50)]
        [DefaultValue(0.5f)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
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
        /// Gets or sets the contrast.
        /// </summary>
        /// <value>The contrast.</value>
        [DataMember(60)]
        [DefaultValue(0f)]
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
        [DataMember(70)]
        [DefaultValue(0f)]
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

            if (Operator == null)
            {
                throw new InvalidOperationException("Operator cannot be null on this instance");
            }

            // Update the luminance
            var elapsedTime = timer.Elapsed;
            timer.Restart();

            var luminanceResult = context.SharedParameters.Get(LuminanceEffect.LuminanceResult);

            var avgLuminanceLog = 0.18f; // TODO: Add a parmetrized average luminance
            if (luminanceResult.LocalTexture != null)
            {
                // Get adapted luminance
                // From "Perceptual effects in real-time tone mapping" by Grzegorz Krawczyk, Karol Myszkowski, Hans-Peter Seidel, p. 3, Equation 5
                var adaptedLum = (float)(previousLuminance + (luminanceResult.AverageLuminance - previousLuminance) * (1.0 - Math.Exp(-elapsedTime.TotalSeconds * AdaptationRate)));
                avgLuminanceLog = (float)Math.Log(adaptedLum, 2);
                previousLuminance = adaptedLum;

                if (AutoKeyValue)
                {
                    // From "Perceptual effects in real-time tone mapping" by Grzegorz Krawczyk, Karol Myszkowski, Hans-Peter Seidel, p. 4, Equation 11
                    KeyValue = 1.03f - (2.0f / (2.0f + (float)Math.Log10(adaptedLum + 1)));
                }
            }

            // Setup parameters
            Parameters.Set(ToneMapShaderKeys.LuminanceTexture, luminanceResult.LocalTexture);
            Parameters.Set(ToneMapShaderKeys.LuminanceAverageGlobal, avgLuminanceLog);

            // Update operator parameters
            Operator.UpdateParameters(context);

            // Copy sub parameters from composition to this transform
            foreach (var key in Operator.Parameters.Keys)
            {
                ParameterKey tonemapKey;
                if (!tonemapKeys.TryGetValue(key, out tonemapKey))
                {
                    tonemapKey = key.ComposeWith("ToneMapOperator");
                    tonemapKeys.Add(key, tonemapKey);
                }
                Operator.Parameters.CopySharedTo(key, tonemapKey, Parameters);
            }
        }
    }
}