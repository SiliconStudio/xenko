// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Base operator shared by Reinhard, Drago, Exponential and Logarithmic.
    /// </summary>
    public abstract class ToneMapCommonOperator : ToneMapOperator
    {
        protected ToneMapCommonOperator(string effectName)
            : base(effectName)
        {
        }

        /// <summary>
        /// Gets or sets the luminance saturation.
        /// </summary>
        /// <value>The luminance saturation.</value>
        public float LuminanceSaturation
        {
            get
            {
                return Parameters.Get(ToneMapCommonOperatorShaderKeys.LuminanceSaturation);
            }
            set
            {
                Parameters.Set(ToneMapCommonOperatorShaderKeys.LuminanceSaturation, value);
            }
        }

        /// <summary>
        /// Gets or sets the white level.
        /// </summary>
        /// <value>The white level.</value>
        public float WhiteLevel
        {
            get
            {
                return Parameters.Get(ToneMapCommonOperatorShaderKeys.WhiteLevel);
            }
            set
            {
                Parameters.Set(ToneMapCommonOperatorShaderKeys.WhiteLevel, value);
            }
        }
    }
}