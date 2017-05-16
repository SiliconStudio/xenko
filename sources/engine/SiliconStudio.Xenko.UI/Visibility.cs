// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Specifies the display state of an element.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Display the element.
        /// </summary>
        /// <userdoc>Display the element.</userdoc>
        Visible,
        /// <summary>
        /// Do not display the element, but reserve space for the element in layout.
        /// </summary>
        /// <userdoc>Do not display the element, but reserve space for the element in layout.</userdoc>
        Hidden,
        /// <summary>
        /// Do not display the element, and do not reserve space for it in layout.
        /// </summary>
        /// <userdoc>Do not display the element, and do not reserve space for it in layout.</userdoc>
        Collapsed,
    }
}
