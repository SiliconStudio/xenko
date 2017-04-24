// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
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
