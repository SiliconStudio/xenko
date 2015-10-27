// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Describes how a child element is vertically positioned or stretched within a parent's layout slot.
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// The child element is aligned to the bottom of the parent's layout slot.
        /// </summary>
        Bottom,
        /// <summary>
        /// The child element is aligned to the center of the parent's layout slot.
        /// </summary>
        Center,
        /// <summary>
        /// The child element is aligned to the top of the parent's layout slot.
        /// </summary>
        Top,
        /// <summary>
        /// The child element stretches to fill the parent's layout slot.
        /// </summary>
        Stretch,
    }
}