// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Base operator shared by Reinhard, Drago, Exponential and Logarithmic.
    /// </summary>
    [DataContract]
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
        [DataMember(5)]
        [DefaultValue(1f)]
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
        [DataMember(8)]
        [DefaultValue(5f)]
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
