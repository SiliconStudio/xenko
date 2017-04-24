// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Indicates where an element should be displayed on the horizontal axis relative to the allocated layout slot of the parent element.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// An element aligned to the left of the layout slot for the parent element.
        /// </summary>
        /// <userdoc>An element aligned to the left of the layout slot for the parent element.</userdoc>
        Left,
        /// <summary>
        /// An element aligned to the center of the layout slot for the parent element.
        /// </summary>
        /// <userdoc>An element aligned to the center of the layout slot for the parent element.</userdoc>
        Center,
        /// <summary>
        /// An element aligned to the right of the layout slot for the parent element.
        /// </summary>
        /// <userdoc>An element aligned to the right of the layout slot for the parent element.</userdoc>
        Right,
        /// <summary>
        /// An element stretched to fill the entire layout slot of the parent element.
        /// </summary>
        /// <userdoc>An element stretched to fill the entire layout slot of the parent element.</userdoc>
        Stretch,
    }
}
