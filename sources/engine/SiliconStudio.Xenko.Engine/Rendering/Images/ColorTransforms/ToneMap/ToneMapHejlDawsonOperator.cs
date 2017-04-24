// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// The tonemap operator by Jim Hejl and Richard Burgess-Dawson.
    /// </summary>
    /// <remarks>http://filmicgames.com/archives/75</remarks>
    [DataContract("ToneMapHejlDawsonOperator")]
    [Display("Hejl-Dawson")]
    public class ToneMapHejlDawsonOperator : ToneMapOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapHejlDawsonOperator"/> class.
        /// </summary>
        public ToneMapHejlDawsonOperator()
            : base("ToneMapHejlDawsonOperatorShader")
        {
        }
    }
}
