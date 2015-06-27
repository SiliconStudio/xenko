// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Represents the different working spaces during rendering
    /// </summary>
    public enum TransformationSpace
    {
        /// <summary>
        /// The absolute world space.
        /// </summary>
        WorldSpace,

        /// <summary>
        /// The space from the object point of view.
        /// </summary>
        ObjectSpace,

        /// <summary>
        /// The space from the camera point of view.
        /// </summary>
        ViewSpace,
    }
}