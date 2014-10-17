// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Defines sprite mirroring options.
    /// </summary>
    /// <remarks>
    /// Description is taken from original XNA <a href='http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.spriteeffects.aspx'>SpriteEffects</a> class.
    /// </remarks>
    [DataContract]
    public enum SpriteEffects
    {
        /// <summary>
        /// No rotations specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Rotate 180 degrees around the Y axis before rendering.
        /// </summary>
        FlipHorizontally = 1,

        /// <summary>
        /// Rotate 180 degrees around the X axis before rendering.
        /// </summary>
        FlipVertically = 3,

        /// <summary>
        /// Rotate 180 degrees around both the X and Y axis before rendering.
        /// </summary>
        FlipBoth = 2,
    };
}