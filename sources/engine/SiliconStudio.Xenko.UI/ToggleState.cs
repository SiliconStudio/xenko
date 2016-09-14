// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describe the different possible states of an <see cref="Controls.ToggleButton"/>.
    /// </summary>
    public enum ToggleState
    {
        /// <summary>
        /// The toggle button is checked.
        /// </summary>
        /// <userdoc>The toggle button is checked.</userdoc>
        Checked,
        /// <summary>
        /// The state of the toggle button is undetermined
        /// </summary>
        /// <userdoc>The state of the toggle button is undetermined</userdoc>
        Indeterminate,
        /// <summary>
        /// The toggle button is unchecked.
        /// </summary>
        /// <userdoc>The toggle button is unchecked.</userdoc>
        UnChecked,
    }
}