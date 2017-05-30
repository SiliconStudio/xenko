// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Describe the possible states of the <see cref="AudioEngine"/>.
    /// </summary>
    public enum AudioEngineState
    {
        /// <summary>
        /// The audio engine is currently running.
        /// </summary>
        Running,

        /// <summary>
        /// The audio engine is currently paused. Any calls to play will be dropped.
        /// </summary>
        Paused,

        /// <summary>
        /// The audio engine is not currently usable due to missing audio hardware or unplugged audio output.
        /// </summary>
        Invalidated,

        /// <summary>
        /// The audio engine is disposed. The current instance cannot be used to play or create sounds anymore.
        /// </summary>
        Disposed,
    }
}
