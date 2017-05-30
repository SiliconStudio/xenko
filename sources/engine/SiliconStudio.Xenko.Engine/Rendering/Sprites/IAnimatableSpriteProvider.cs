// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
