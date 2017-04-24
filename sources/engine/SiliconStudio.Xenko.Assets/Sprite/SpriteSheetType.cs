// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Sprite
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
