// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A bright pass filter.
    /// </summary>
    public class BrightFilter : ImageEffectShader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrightFilter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="brightPassShaderName">Name of the bright pass shader.</param>
        public BrightFilter(ImageEffectContext context, string brightPassShaderName = "BrightFilterShader")
            : base(context, brightPassShaderName)
        {
        }

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
    }
}