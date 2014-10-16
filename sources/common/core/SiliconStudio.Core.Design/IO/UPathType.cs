// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// Describes if a <see cref="UPath"/> is relative or absolute.
    /// </summary>
    public enum UPathType
    {
        /// <summary>
        /// The path is absolute
        /// </summary>
        Absolute,

        /// <summary>
        /// The path is relative
        /// </summary>
        Relative,
    }
}