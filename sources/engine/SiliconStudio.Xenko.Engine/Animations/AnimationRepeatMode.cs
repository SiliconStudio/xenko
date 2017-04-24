// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Animations
{
    /// <summary>
    /// Enumeration describing how an animation should be repeated.
    /// </summary>
    [DataContract]
    public enum AnimationRepeatMode
    {
        /// <summary>
        /// The animation plays once, and then stops.
        /// </summary>
        /// <userdoc>The animation plays once, and then stops.</userdoc>
        [Display("Play once")]
        PlayOnce,

        /// <summary>
        /// The animation loop for always.
        /// </summary>
        /// <userdoc>The animation loop for always.</userdoc>
        [Display("Loop")]
        LoopInfinite,

        /// <summary>
        /// The animation plays once, and then holds, being kept in the list.
        /// </summary>
        /// <userdoc>The animation plays once, and then holds, being kept in the list.</userdoc>
        [Display("Play once & hold")]
        PlayOnceHold,
    }
}
