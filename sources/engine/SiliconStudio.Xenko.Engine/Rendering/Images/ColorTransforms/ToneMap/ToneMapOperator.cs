// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Images
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