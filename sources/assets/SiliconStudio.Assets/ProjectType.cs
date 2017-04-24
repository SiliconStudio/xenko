// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    // REMARK: Beware of the order of values in this enum, it is used for sorting.

    /// <summary>
    /// Type of the project.
    /// </summary>
    [DataContract("ProjectType")]
    public enum ProjectType
    {
        /// <summary>
        /// A library.
        /// </summary>
        Library,

        /// <summary>
        /// An executable.
        /// </summary>
        Executable,

        /// <summary>
        /// A plugin.
        /// </summary>
        Plugin,
    }
}
