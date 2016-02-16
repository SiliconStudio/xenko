// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// The tonemap Drago operator.
    /// </summary>
    [DataContract("ToneMapDragoOperator")]
    [Display("Drago")]
    public class ToneMapDragoOperator : ToneMapCommonOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapDragoOperator"/> class.
        /// </summary>
        public ToneMapDragoOperator()
            : base("ToneMapDragoOperatorShader")
        {
        }

        /// <summary>
        /// Gets or sets the bias.
        /// </summary>
        /// <value>The bias.</value>
        [DataMember(10)]
        [DefaultValue(0.5f)]
        public float Bias
        {
            get
            {
                return Parameters.GetValueSlow(ToneMapDragoOperatorShaderKeys.DragoBias);
            }
            set
            {
                Parameters.SetValueSlow(ToneMapDragoOperatorShaderKeys.DragoBias, value);
            }
        }
    }
}