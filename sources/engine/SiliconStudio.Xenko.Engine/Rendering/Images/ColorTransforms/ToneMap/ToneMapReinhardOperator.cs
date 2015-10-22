// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// The tonemap Reinhard operator.
    /// </summary>
    [DataContract("ToneMapReinhardOperator")]
    [Display("Reinhard")]
    public class ToneMapReinhardOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapReinhardOperator"/> class.
        /// </summary>
        public ToneMapReinhardOperator()
            : base("ToneMapReinhardOperatorShader")
        {
        }
    }
}