// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Rendering.Sprites
{
    /// <summary>
    /// A sprite provider from a <see cref="SpriteGroup"/>
    /// </summary>
    [DataContract("SpriteFromSpriteGroup")]
    [Display("Sprite Group")]
    public class SpriteFromSpriteGroup : ISpriteProvider
    {
        /// <summary>
        /// Gets or sets the <see cref="SpriteGroup"/> of the provider.
        /// </summary>
        public SpriteGroup SpriteGroup { get; set; }

        public Sprite GetSprite(int index)
        {
            if (SpriteGroup == null || SpriteGroup.Images == null || SpriteGroup.Images.Count == 0)
                return null;

            return SpriteGroup.Images[index % SpritesCount];
        }

        public int SpritesCount
        {
            get
            {
                if (SpriteGroup != null && SpriteGroup.Images != null)
                    return SpriteGroup.Images.Count;

                return 0;
            }
        }
    }
}