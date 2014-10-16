// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// The different types of strip possible of a grid.
    /// </summary>
    public enum StripType
    {
        /// <summary>
        /// A strip having fixed size expressed in number of virtual pixels.
        /// </summary>
        Fixed,

        /// <summary>
        /// A strip that occupies exactly the size required by its content. 
        /// </summary>
        Auto,

        /// <summary>
        /// A strip that occupies the maximum available size, dispatched among the other stared-size columns.
        /// </summary>
        Star,
    }
}