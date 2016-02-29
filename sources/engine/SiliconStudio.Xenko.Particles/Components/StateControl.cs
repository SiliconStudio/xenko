// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Components
{
    /// <summary>
    /// State control for the particle system
    /// </summary>
    [DataContract]
    public enum StateControl
    {
        /// <summary>
        /// The state is active and currently playing
        /// </summary>
        Play,

        /// <summary>
        /// The state is active, but currently not playing (paused)
        /// </summary>
        Pause,

        /// <summary>
        /// The state is inactive
        /// </summary>
        Stop,
    }

}
