// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describes the action of a specific touch point.
    /// </summary>
    public enum TouchAction
    {
        /// <summary>
        /// The act of putting a finger onto the screen.
        /// </summary>
        /// <userdoc>The act of putting a finger onto the screen.</userdoc>
        Down,
        /// <summary>
        /// The act of dragging a finger across the screen.
        /// </summary>
        /// <userdoc>The act of dragging a finger across the screen.</userdoc>
        Move,
        /// <summary>
        /// The act of lifting a finger off of the screen.
        /// </summary>
        /// <userdoc>The act of lifting a finger off of the screen.</userdoc>
        Up,
    }
}
