// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A bright pass filter.
    /// </summary>
    [DataContract("BrightFilter")]
    public class BrightFilter : ImageEffectShader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrightFilter"/> class.
        /// </summary>
        /// <param name="brightPassShaderName">Name of the bright pass shader.</param>
        public BrightFilter(string brightPassShaderName = "BrightFilterShader")
        {
            EffectName = brightPassShaderName;
        }

        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        /// <value>The threshold.</value>
        [DataMember(10)]
        [DefaultValue(2.0)]
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