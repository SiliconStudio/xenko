// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// A shader source.
    /// </summary>
    [DataContract("ShaderSource")]
    public abstract class ShaderSource
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ShaderSource"/> is a discard shader after it has been mixed.
        /// </summary>
        /// <value><c>true</c> if discard; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool Discard { get; set; }

        /// <summary>
        /// Deep clones this instance.
        /// </summary>
        /// <returns>A new instance.</returns>
        public abstract object Clone();

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="against">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public abstract override bool Equals(object against);

        public abstract override int GetHashCode();
    }
}