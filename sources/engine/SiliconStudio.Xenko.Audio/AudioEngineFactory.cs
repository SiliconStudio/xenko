// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Audio
{
    public static class AudioEngineFactory
    {
        /// <summary>
        /// Based on compilation setting, returns the proper instance of sounds.
        /// </summary>
        /// <returns>A platform specific instance of <see cref="AudioEngine"/></returns>
        public static AudioEngine NewAudioEngine()
        {
            AudioEngine engine = null;
#if SILICONSTUDIO_PLATFORM_IOS
            engine = new AudioEngineIos();
#else
            engine = new AudioEngine();
#endif
            engine.InitializeAudioEngine();
            return engine;
        }
    }
}
