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
#if SILICONSTUDIO_PLATFORM_ANDROID
            return new AudioEngineAndroid();
#elif SILICONSTUDIO_PLATFORM_IOS        
            var engine = new AudioEngineIos();
            engine.InitializeAudioEngine();
            return engine;
#elif SILICONSTUDIO_PLATFORM_WINDOWS
#if SILICONSTUDIO_XENKO_SOUND_SDL
            return new AudioEngineSDL();
#elif SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            return new AudioEngineRuntime();
#else
            var engine = new AudioEngine();
            engine.InitializeAudioEngine();
            return engine;
#endif
#elif SILICONSTUDIO_PLATFORM_LINUX
#if SILICONSTUDIO_XENKO_SOUND_SDL
            return new AudioEngineSDL();
#else
            return null;
#endif
#else
            return null;
#endif
        }
    }
}
