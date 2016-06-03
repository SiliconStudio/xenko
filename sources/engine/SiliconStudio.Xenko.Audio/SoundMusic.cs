// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Threading;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// This class provides a sound resource which is playable but not localizable. 
    /// <para>
    /// SoundMusics are usually "long" sounds that need neither to be localized nor to be played with low latency. 
    /// Classical examples are background music or explanations. SoundMusics are progressively streamed to minimize memory usage.
    /// The user can also reduce the assets global size using the MP3 file format to encode SoundMusic.
    /// If low latency or sound localization is required take a look at the <see cref="SoundEffect"/> or <see cref="DynamicSoundEffectInstance"/> classes.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>Only one instance of SoundMusic can be played at a time. Thus, playing a new SoundMusic stops the previous instance.</para>
    /// <para>
    /// You can create a SoundMusics by calling the static <see cref="Load"/> load function. 
    /// Currently only mono and stereo wav and mp3 files are supported for SoundMusics.
    ///  </para>
    /// </remarks>
    /// <seealso cref="SoundEffect"/>
    /// <seealso cref="IPlayableSound"/>
    /// <seealso cref="DynamicSoundEffectInstance"/>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<SoundMusic>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<SoundMusic>), Profile = "Content")]
    public sealed class SoundMusic : SoundInstance
    {
#if SILICONSTUDIO_PLATFORM_ANDROID
        internal string FileName;
        internal long StartPosition;
        internal long Length;
#else
        internal MemoryStream Stream;
#endif

        /// <summary>
        /// The number of SoundMusic Created so far. Used only to give a unique name to the SoundEffect.
        /// </summary>
        private static int soundMusicCreationCount;

        private bool contentIsReady;

        public void Attach(AudioEngine engine)
        {
            if (contentIsReady) return;

            AttachEngine(engine);

            ResetStateToDefault();

            Name = "SoundMusic " + soundMusicCreationCount;

            AudioEngine.RegisterSound(this);

            Interlocked.Increment(ref soundMusicCreationCount);

            contentIsReady = true;
        }


        private void ResetStateToDefault()
        {
            Volume = 1;
            IsLooped = false;
            Stop();
        }

        private static SoundMusic previousPlayingInstance;
        private static readonly object PreviousPlayingInstanceLock = new object();

        internal override void PlayImpl()
        {
            AudioEngine.SubmitMusicActionRequest(new SoundMusicActionRequest(this, SoundMusicAction.Play));

            // Actual Playing is happening during the Audio Engine update
            // but we can not wait this long to update the PlayState of the currently playing SoundMusic
            // after this call to Play, PlayState of the previous playing music should directly be set to Stopped
            // this is why we use here the static field PreviousPlayingInstance
            lock (PreviousPlayingInstanceLock) // protection again possible future multithreading.
            {
                if (previousPlayingInstance != this)
                {
                    if (previousPlayingInstance != null)
                        previousPlayingInstance.SetStateToStopped();

                    previousPlayingInstance = this;
                }
            }
        }

        internal override void PauseImpl()
        {
            AudioEngine.SubmitMusicActionRequest(new SoundMusicActionRequest(this, SoundMusicAction.Pause));
        }

        internal override void StopImpl()
        {
            ShouldExitLoop = false;

            AudioEngine.SubmitMusicActionRequest(new SoundMusicActionRequest(this, SoundMusicAction.Stop));
        }

        internal override void UpdateVolume()
        {
            AudioEngine.SubmitMusicActionRequest(new SoundMusicActionRequest(this, SoundMusicAction.Volume));
        }

        internal override void UpdateLooping()
        {
            // Nothing to do. 
            // music looping system is directly based on the value of SoundMusic.IsLooped. 
        }

        internal void SetStateToStopped()
        {
            PlayState = SoundPlayState.Stopped;
            DataBufferLoaded = false;
            ShouldExitLoop = false;
        }

        internal bool ShouldExitLoop { get; private set; }

        internal override void ExitLoopImpl()
        {
            ShouldExitLoop = true;
        }

        internal override void DestroyImpl()
        {
            AudioEngine.UnregisterSound(this);
            // mediaInputStream is disposed by AudioEngine.ProcessPlayerClosed()
        }
    }
}
