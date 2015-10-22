// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// The different ways of scrolling in a <see cref="ScrollViewer"/>.
    /// </summary>
    public enum ScrollingMode
    {
        /// <summary>
        /// No scrolling is allowed.
        /// </summary>
        None,
        /// <summary>
        /// Only horizontal scrolling is allowed.
        /// </summary>
        Horizontal,
        /// <summary>
        /// Only vertical scrolling is allowed.
        /// </summary>
        Vertical,
        /// <summary>
        /// Only in depth (back/front) scrolling is allowed.
        /// </summary>
        InDepth,
        /// <summary>
        /// Both horizontal and vertical scrolling are allowed.
        /// </summary>
        HorizontalVertical,
        /// <summary>
        /// Both vertical and in-depth scrolling are allowed.
        /// </summary>
        VerticalInDepth,
        /// <summary>
        /// Both in-depth and horizontal scrolling are allowed.
        /// </summary>
        InDepthHorizontal,
    }
}