// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Specifies the display state of an element.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Display the element.
        /// </summary>
        Visible,

        /// <summary>
        /// Do not display the element, but reserve space for the element in layout.
        /// </summary>
        Hidden,

        /// <summary>
        /// Do not display the element, and do not reserve space for it in layout.
        /// </summary>
        Collapsed,
    }
}