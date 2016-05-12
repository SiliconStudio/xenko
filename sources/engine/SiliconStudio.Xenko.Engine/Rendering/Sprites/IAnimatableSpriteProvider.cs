// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// The base interface for all classes providing animated sprites.
    /// </summary>
    [InlineProperty]
    public interface IAnimatableSpriteProvider : ISpriteProvider
    {
        /// <summary>
        /// Gets or sets the current frame of the animation.
        /// </summary>
        int CurrentFrame { get; set; }
    }
}
