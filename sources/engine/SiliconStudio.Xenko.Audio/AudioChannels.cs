// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Enumeration containing the different audio output configurations.
    /// </summary>
    /// <remarks>Currently only Mono and Stereo sounds are supported</remarks>
    /// <seealso cref="DynamicSoundEffectInstance"/>
    public enum AudioChannels
    {
        /// <summary>
        /// A 1-channel mono sounds.
        /// </summary>
        Mono = 1,

        /// <summary>
        /// A 2-channels stereo sounds.
        /// </summary>
        Stereo = 2,
    }
}
