// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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

        public Sprite GetSprite(int index)
        {
            var count = SpritesCount;
            return count > 0 ? Sheet.Sprites[(index % count + count) % count] : null; // in case of a negative index, it will cycle around
        }

        public int SpritesCount => Sheet?.Sprites.Count ?? 0;
    }
}
