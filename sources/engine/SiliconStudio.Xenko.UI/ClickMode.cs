// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Specifies when the Click event should be raised.
    /// </summary>
    public enum ClickMode
    {
        /// <summary>
        /// Specifies that the Click event should be raised as soon as a button is pressed.
        /// </summary>
        Press,
        /// <summary>
        /// Specifies that the Click event should be raised when a button is pressed and released.
        /// </summary>
        Release,
    }
}