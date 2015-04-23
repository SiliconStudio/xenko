// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Sprites
{
    /// <summary>
    /// Enumerates the different ways to extrude an <see cref="Sprite"/>
    /// </summary>
    public enum SpriteExtrusionMethod
    {
        /// <summary>
        /// A unit rectangle
        /// </summary>
        /// <userdoc>The sprite is extruded as a unit rectangle along x and y</userdoc>
        UnitRectangle,

        /// <summary>
        /// A rectangle having a unit width and preserving the sprite ratio.
        /// </summary>
        /// <userdoc>The sprite is extruded as a rectangle having a unit width and preserving the sprite ratio</userdoc>
        UnitWidthSpriteRatio,

        /// <summary>
        /// A rectangle having a unit height and preserving the sprite ratio.
        /// </summary>
        /// <userdoc>The sprite is extruded as a rectangle having a unit height and preserving the sprite ratio</userdoc>
        UnitHeightSpriteRatio,
    }
}