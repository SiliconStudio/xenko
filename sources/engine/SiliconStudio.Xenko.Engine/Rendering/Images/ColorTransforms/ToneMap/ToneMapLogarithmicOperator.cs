// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// The tonemap logarithmic operator.
    /// </summary>
    [DataContract("ToneMapLogarithmicOperator")]
    [Display("Logarithmic")]
    public class ToneMapLogarithmicOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapLogarithmicOperator"/> class.
        /// </summary>
        public ToneMapLogarithmicOperator()
            : base("ToneMapLogarithmicOperatorShader")
        {
        }
    }
}
