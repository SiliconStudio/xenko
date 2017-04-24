// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Reprensent an Audio Hardware Device.
    /// Can be used when creating an <see cref="AudioEngine"/> to specify the device on which to play the sound.
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Returns the name of the current device.
        /// </summary>
        public string Name { get; set; }

        public AudioDevice()
        {
            Name = "default";
        }
    }
}
