// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// The base interface for all classes providing sprites.
    /// </summary>
    [InlineProperty]
    public interface ISpriteProvider
    {
        /// <summary>
        /// Gets the number of sprites available in the provider.
        /// </summary>
        int SpritesCount { get; }

        /// <summary>
        /// Get a sprite from the provider.
        /// </summary>
        /// <returns></returns>
        Sprite GetSprite();
    }
}
