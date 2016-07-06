// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A SourceSpan.
    /// </summary>
    [DataContract]
    public struct SourceSpan
    {
        #region Constants and Fields

        /// <summary>
        /// Location of this span.
        /// </summary>
        public SourceLocation Location;

        /// <summary>
        /// Length of this span.
        /// </summary>
        public int Length;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceSpan"/> struct.
        /// </summary>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        public SourceSpan(SourceLocation location, int length)
        {
            Location = location;
            Length = length;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0}", Location);
        }

        #endregion
    }
}