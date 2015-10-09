// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Sprite
{
    /// <summary>
    /// The different types of the sprite sheets.
    /// </summary>
    public enum SpriteSheetType
    {
        /// <summary>
        /// A sprite sprite sheet designed for 2D sprites.
        /// </summary>
        /// <userdoc>A sprite sprite sheet designed for 2D sprites.</userdoc>
        [Display("Sprite sheet for 2D sprites")]
        Sprite2D,

        /// <summary>
        /// A sprite sheet designed for UI.
        /// </summary>
        /// <userdoc>A sprite sheet designed for UI.</userdoc>
        [Display("Sprite sheet for UI")]
        UI,
    }
}