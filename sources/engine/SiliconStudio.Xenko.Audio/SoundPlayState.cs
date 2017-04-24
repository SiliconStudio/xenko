// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
