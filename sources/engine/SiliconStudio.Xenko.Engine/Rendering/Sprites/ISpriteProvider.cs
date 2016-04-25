// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// The base interface for all classes providing a sequence of sprites.
    /// </summary>
    [InlineProperty]
    public interface ISpriteProvider
    {
        /// <summary>
        /// Gets or sets the current frame of the animation.
        /// </summary>
        int CurrentFrame { get; set; }

        /// <summary>
        /// Gets the number of sprites available in the sequence.
        /// </summary>
        int SpritesCount { get; }

        /// <summary>
        /// Get the sprite located at <paramref name="index"/> in the sprite sequence.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Sprite GetSprite(int index);
    }
}
