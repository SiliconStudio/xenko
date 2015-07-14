// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Sprites
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
            if (Sheet == null || Sheet.Sprites == null || Sheet.Sprites.Count == 0)
                return null;

            return Sheet.Sprites[index % SpritesCount];
        }

        public int SpritesCount
        {
            get
            {
                if (Sheet != null && Sheet.Sprites != null)
                    return Sheet.Sprites.Count;

                return 0;
            }
        }
    }
}