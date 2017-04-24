// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// The different types of strip possible of a grid.
    /// </summary>
    public enum StripType
    {
        /// <summary>
        /// A strip having fixed size expressed in number of virtual pixels.
        /// </summary>
        /// <userdoc>A strip having fixed size expressed in number of virtual pixels.</userdoc>
        Fixed,

        /// <summary>
        /// A strip that occupies exactly the size required by its content. 
        /// </summary>
        /// <userdoc>A strip that occupies exactly the size required by its content. </userdoc>
        Auto,

        /// <summary>
        /// A strip that occupies the maximum available size, dispatched among the other stared-size columns.
        /// </summary>
        /// <userdoc>A strip that occupies the maximum available size, dispatched among the other stared-size columns.</userdoc>
        Star,
    }
}
