// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Current state (playing, paused, or stopped) of a sound implementing the <see cref="IPlayableSound"/> interface.
    /// </summary>
    /// <seealso cref="IPlayableSound"/>
    public enum SoundPlayState
    {
        /// <summary>
        /// The sound is currently being played.
        /// </summary>
        Playing,

        /// <summary>
        /// The sound is currently paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The sound is currently stopped.
        /// </summary>
        Stopped,
    }
}