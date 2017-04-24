// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// The tonemap exponential operator.
    /// </summary>
    [DataContract("ToneMapExponentialOperator")]
    [Display("Exponential")]
    public class ToneMapExponentialOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapExponentialOperator"/> class.
        /// </summary>
        public ToneMapExponentialOperator()
            : base("ToneMapExponentialOperatorShader")
        {
        }
    }
}
