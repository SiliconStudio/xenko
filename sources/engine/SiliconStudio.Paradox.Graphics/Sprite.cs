// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A sprite represents a series frames in an atlas forming an animation. 
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<Sprite>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Sprite>), Profile = "Asset")]
    public class Sprite : ImageFragment
    {
        /// <summary>
        /// Creates a new instance of sprite with unique random name.
        /// </summary>
        public Sprite()
            : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Create a new instance of sprite.
        /// </summary>
        /// <param name="fragmentName">the sprite name</param>
        public Sprite(string fragmentName)
            : base(fragmentName)
        {
        }

        /// <summary>
        /// The position of the center of the image in pixels.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Draw a specific frame of the sprite with white color and scale of 1.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to draw the sprite.</param>
        /// <param name="position">The position to which draw the sprite</param>
        /// <param name="rotation">The rotation to apply on the sprite</param>
        /// <param name="depthLayer">The depth layer to which draw the sprite</param>
        /// <param name="spriteEffects">The sprite effect to apply on the sprite</param>
        /// <remarks>This function must be called between the <see cref="SpriteBatch.Begin(SiliconStudio.Paradox.Graphics.SpriteSortMode,SiliconStudio.Paradox.Graphics.Effect)"/> 
        /// and <see cref="SpriteBatch.End()"/> calls of the provided <paramref name="spriteBatch"/></remarks>
        /// <exception cref="ArgumentException">The provided frame index is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided spriteBatch is null</exception>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation = 0, float depthLayer = 0, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            Draw(spriteBatch, position, Color.White, Vector2.One, rotation, depthLayer, spriteEffects);
        }

        /// <summary>
        /// Draw a specific frame of the sprite.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to draw the sprite.</param>
        /// <param name="position">The position to which draw the sprite</param>
        /// <param name="color">The color to use to draw the sprite</param>
        /// <param name="rotation">The rotation to apply on the sprite</param>
        /// <param name="scales">The scale factors to apply on the sprite</param>
        /// <param name="depthLayer">The depth layer to which draw the sprite</param>
        /// <param name="spriteEffects">The sprite effect to apply on the sprite</param>
        /// <remarks>This function must be called between the <see cref="SpriteBatch.Begin(SiliconStudio.Paradox.Graphics.SpriteSortMode,SiliconStudio.Paradox.Graphics.Effect)"/> 
        /// and <see cref="SpriteBatch.End()"/> calls of the provided <paramref name="spriteBatch"/></remarks>
        /// <exception cref="ArgumentException">The provided frame index is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided spriteBatch is null</exception>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 scales, float rotation = 0f, float depthLayer = 0, SpriteEffects spriteEffects = SpriteEffects.None)
        {
            if (spriteBatch == null) throw new ArgumentNullException("spriteBatch");
        
            if(Texture == null)
                return;

            spriteBatch.Draw(Texture, position, Region, color, rotation, Center, scales, spriteEffects, Orientation, depthLayer);
        }

        /// <summary>
        /// Clone the current sprite.
        /// </summary>
        /// <returns>A new instance of the current sprite.</returns>
        public Sprite Clone()
        {
            return (Sprite)MemberwiseClone();
        }
    }
}