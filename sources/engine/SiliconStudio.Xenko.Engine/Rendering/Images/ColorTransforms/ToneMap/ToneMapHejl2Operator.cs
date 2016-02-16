// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// The tonemap operator by Jim Hejl version 2 that does not include the gamma correction and has a whitepoint parameter.
    /// </summary>
    /// <remarks>
    /// https://twitter.com/jimhejl/status/633777619998130176
    /// </remarks>
    [DataContract("ToneMapHejl2Operator")]
    [Display("Hejl2")]
    public class ToneMapHejl2Operator : ToneMapOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapHejlDawsonOperator"/> class.
        /// </summary>
        public ToneMapHejl2Operator()
            : base("ToneMapHejl2OperatorShader")
        {
        }

        /// <summary>
        /// Gets or sets the white point.
        /// </summary>
        /// <value>The white point.</value>
        [DataMember(10)]
        [DefaultValue(5.0f)]
        public float WhitePoint
        {
            get
            {
                return Parameters.GetValueSlow(ToneMapHejl2OperatorShaderKeys.WhitePoint);
            }
            set
            {
                Parameters.SetValueSlow(ToneMapHejl2OperatorShaderKeys.WhitePoint, value);
            }
        }
    }
}