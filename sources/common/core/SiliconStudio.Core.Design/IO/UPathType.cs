// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
