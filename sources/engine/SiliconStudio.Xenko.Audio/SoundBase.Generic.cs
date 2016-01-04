// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if !SILICONSTUDIO_PLATFORM_WINDOWS
namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundBase
    {
        /// <summary>
        /// Create the audio engine to the sound base instance.
        /// </summary>
        /// <param name="engine">A valid AudioEngine</param>
        /// <exception cref="ArgumentNullException">The engine argument is null</exception>
        internal void AttachEngine(AudioEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            AudioEngine = engine;
        }

        internal AudioEngine AudioEngine { get; private set; }
    }
}
#endif