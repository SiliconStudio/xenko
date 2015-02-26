// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A <see cref="Sprite"/> provider from a <see cref="Texture"/>.
    /// </summary>
    [DataContract]
    [Display("Texture")]
    public class SpriteFromTexture : ISpriteProvider
    {
        private Texture previousTexture;
        private Texture texture;
        private Sprite sprite;

        /// <summary>
        /// The position of the center of the image in pixels.
        /// </summary>
        /// <userdoc>
        /// The position of the center of the sprite in pixels. 
        /// Depending on the value of 'CenterFromMiddle', it is the offset from the top/left corner or the middle of the image.
        /// </userdoc>
        [DataMember(10)]
        public Vector2 Center;

        /// <summary>
        /// Gets or sets the value indicating position provided to <see cref="Center"/> is from the middle of the sprite region or from the left/top corner.
        /// </summary>
        /// <userdoc>
        /// If checked, the value in 'Center' represents the offset of the sprite center from the middle of the image.
        /// </userdoc>
        [DataMember(15)]
        [DefaultValue(true)]
        public bool CenterFromMiddle { get; set; }
        
        /// <summary>
        /// Gets or sets the transparency value of the sprite.
        /// </summary>
        /// <userdoc>
        /// If checked, the sprite is considered as having transparent colors.
        /// </userdoc>
        [DataMember(20)]
        [DefaultValue(true)]
        public bool IsTransparent { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="SpriteFromTexture"/>.
        /// </summary>
        public SpriteFromTexture()
        {
            CenterFromMiddle = true;
            IsTransparent = true;
        }

        /// <summary>
        /// The texture of representing the sprite
        /// </summary>
        [DataMember(5)]
        public Texture Texture
        {
            get { return texture; }
            set
            {
                previousTexture = texture;
                texture = value;
            }
        }

        public Sprite GetSprite(int index)
        {
            // regenerate the sprite if the texture changed.
            if (previousTexture != texture)
            {
                sprite = null;
                if (texture != null)
                {
                    sprite = new Sprite
                    {
                        Texture = Texture,
                        Center = Center + (CenterFromMiddle ? new Vector2(texture.Width, texture.Height) : Vector2.Zero),
                        Region = new RectangleF(0, 0, texture.Width, texture.Height),
                        IsTransparent = IsTransparent
                    };
                }
            }

            previousTexture = texture;

            return sprite;
        }

        public int SpritesCount { get { return sprite == null? 0: 1; } }
    }
}