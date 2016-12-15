// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// Type which describes the nature of the animation clip we want to use.
    /// The terms are borrowed from the book Game Engine Architecture, Chapter 11.6.5 Additive Blending
    /// </summary>
    [DataContract]
    public enum AnimationClipBlendMode
    {
        /// <summary>
        /// Standard animation clip which animates the character
        /// </summary>
        [Display("Animation Clip")]
        LinearBlend = 0,

        /// <summary>
        /// Difference animation clip which is added on top of another pose
        /// </summary>
        [Display("Difference Clip")]
        Additive = 1,
    }
}
