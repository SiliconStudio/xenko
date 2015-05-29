// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// The base interface for all classes providing a sequence of sprites.
    /// </summary>
    public interface ISpriteProvider
    {
        /// <summary>
        /// Get the sprite located at <paramref name="index"/> in the sprite sequence.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Sprite GetSprite(int index);

        /// <summary>
        /// Gets the number of sprites available in the sequence.
        /// </summary>
        int SpritesCount { get; }
    }
}