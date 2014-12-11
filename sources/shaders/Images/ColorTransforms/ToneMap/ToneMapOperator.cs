// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.


namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Base class for a tonemap operator.
    /// </summary>
    public abstract class ToneMapOperator : ColorTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToneMapOperator"/> class.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <exception cref="System.ArgumentNullException">effectName</exception>
        protected ToneMapOperator(string effectName) : base(effectName)
        {
        }
    }
}