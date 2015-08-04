// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Animations
{
    /// <summary>
    /// Enumeration describing how an animation should be repeated.
    /// </summary>
    [DataContract]
    public enum AnimationRepeatMode
    {
        /// <summary>
        /// The animation play once, and then stops.
        /// </summary>
        /// <userdoc>The animation play once, and then stops.</userdoc>
        PlayOnce,

        /// <summary>
        /// The animation loop for always.
        /// </summary>
        /// <userdoc>The animation loop for always.</userdoc>
        LoopInfinite,
    }
}