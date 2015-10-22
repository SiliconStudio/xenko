// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Shaders
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