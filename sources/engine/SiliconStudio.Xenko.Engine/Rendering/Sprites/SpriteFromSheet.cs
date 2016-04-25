// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    /// <summary>
    /// A sprite provider from a <see cref="Sheet"/>
    /// </summary>
    [DataContract("SpriteFromSheet")]
    [Display("Sprite Group")]
    public class SpriteFromSheet : ISpriteProvider
    {
        /// <summary>
        /// Gets or sets the <see cref="Sheet"/> of the provider.
        /// </summary>
        /// <userdoc>The sheet that provides the sprites</userdoc>
        [InlineProperty]
        public SpriteSheet Sheet { get; set; }

        /// <summary>
        /// Gets or sets the current frame of the animation.
        /// </summary>
        /// <userdoc>The index of the default frame of the sprite sheet to use.</userdoc>
        [DefaultValue(0)]
        [Display("Default Frame")]
        public int CurrentFrame { get; set; }

        /// <inheritdoc/>
        public int SpritesCount => Sheet?.Sprites.Count ?? 0;

        /// <inheritdoc/>
        public Sprite GetSprite()
        {
            return SpritesCount != 0 ? Sheet.Sprites[CurrentFrame % SpritesCount] : null;
        }
    }
}
