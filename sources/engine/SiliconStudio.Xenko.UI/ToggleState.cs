// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describe the different possible states of an <see cref="ToggleButton"/>.
    /// </summary>
    public enum ToggleState
    {
        /// <summary>
        /// The toggle button is checked.
        /// </summary>
        Checked,

        /// <summary>
        /// The state of the toggle button is undetermined
        /// </summary>
        Indeterminate,

        /// <summary>
        /// The toggle button is unchecked.
        /// </summary>
        UnChecked,
    }
}