// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Audio
{
    public static class AudioEngineFactory
    {
        /// <summary>
        /// Based on compilation setting, returns the proper instance of sounds.
        /// </summary>
        /// <returns>A platform specific instance of <see cref="AudioEngine"/></returns>
        public static AudioEngine NewAudioEngine(AudioDevice device = null, AudioLayer.DeviceFlags deviceFlags = AudioLayer.DeviceFlags.None)
        {
            AudioEngine engine = null;
#if SILICONSTUDIO_PLATFORM_IOS
            engine = new AudioEngineIos();
#else
            engine = new AudioEngine(device);
#endif
            engine.InitializeAudioEngine(deviceFlags);
            return engine;
        }
    }
}
