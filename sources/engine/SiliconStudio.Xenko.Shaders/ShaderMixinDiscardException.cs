// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// An exception used to early exit a shader mixin with discard.
    /// </summary>
    public class ShaderMixinDiscardException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinDiscardException"/> class.
        /// </summary>
        public ShaderMixinDiscardException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinDiscardException"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ShaderMixinDiscardException(ShaderSource source)
        {
            DiscardSource = source;
        }

        /// <summary>
        /// Gets the discard source if any (may be null).
        /// </summary>
        /// <value>The discard source.</value>
        public ShaderSource DiscardSource { get; private set; }
    }
}
