// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Images
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