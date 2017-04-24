// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describe the possible states of the mouse over an UI element.
    /// </summary>
    public enum MouseOverState
    {
        /// <summary>
        /// The mouse is neither over the element nor one of its children.
        /// </summary>
        /// <userdoc>The mouse is neither over the element nor one of its children.</userdoc>
        MouseOverNone,
        /// <summary>
        /// The mouse is over one of children of the element.
        /// </summary>
        /// <userdoc>The mouse is over one of children of the element.</userdoc>
        MouseOverChild,
        /// <summary>
        /// The mouse is directly over the element.
        /// </summary>
        /// <userdoc>The mouse is directly over the element.</userdoc>
        MouseOverElement,
    }
}
