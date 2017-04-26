// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Base class for a tonemap operator.
    /// </summary>
    [DataContract]
    public abstract class ToneMapOperator : ColorTransformBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapOperator"/> class.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <exception cref="System.ArgumentNullException">effectName</exception>
        protected ToneMapOperator(string effectName) : base(effectName)
        {
        }

        [DataMemberIgnore]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }
    }
}
