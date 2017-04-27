// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describes how a child element is vertically positioned or stretched within a parent's layout slot.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// The child element is aligned to the bottom of the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element is aligned to the bottom of the parent's layout slot.</userdoc>
        Bottom,
        /// <summary>
        /// The child element is aligned to the center of the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element is aligned to the center of the parent's layout slot.</userdoc>
        Center,
        /// <summary>
        /// The child element is aligned to the top of the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element is aligned to the top of the parent's layout slot.</userdoc>
        Top,
        /// <summary>
        /// The child element stretches to fill the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element stretches to fill the parent's layout slot.</userdoc>
        Stretch,
    }
}
