// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace SiliconStudio.Paradox.Audio
{
    partial class AudioEngine
    {
        private MediaEngine mediaEngine;
        private MediaEngineEx mediaEngineEx;

        private void PlatformSpecificInit()
        {
            // Setup Media Engine attributes
            using (var attributes = new MediaEngineAttributes { AudioEndpointRole = AudioEndpointRole.Console,
                                                                AudioCategory = AudioStreamCategory.GameEffects })
            {
                var creationFlags = MediaEngineCreateFlags.None;
#if !SILICONSTUDIO_PLATFORM_WINDOWS_PHONE
                // MSDN: On the phone, the Media Engine only supports frame-server mode. Attempting to initialize the interface in either rendering mode or audio mode will fail.
                creationFlags |= MediaEngineCreateFlags.AudioOnly;
#endif
                using (var factory = new MediaEngineClassFactory())
                    mediaEngine = new MediaEngine(factory, attributes, creationFlags, OnMediaEngineEvent);

                mediaEngineEx = mediaEngine.QueryInterface<MediaEngineEx>();
            }
        }

        private void PlatformSpecificDispose()
        {
            mediaEngine.Shutdown();
            mediaEngine.Dispose();
        }

        private void ResetMusicPlayer()
        {
            StopCurrentMusic();

            isMusicPlayerReady = false;

            currentMusic = null;
        }

        private void RestartCurrentMusic()
        {
            mediaEngine.CurrentTime = 0;
        }

        private void StartCurrentMusic()
        {
            mediaEngine.Play();
        }

        private void UpdateMusicVolume()
        {
            mediaEngine.Volume = currentMusic.Volume;
        }

        private void StopCurrentMusic()
        {
            PauseCurrentMusic();
            RestartCurrentMusic();
        }

        private void PauseCurrentMusic()
        {
            mediaEngine.Pause();
        }
        
        private void LoadNewMusic(SoundMusic lastPlayRequestMusicInstance)
        {
            currentMusic = lastPlayRequestMusicInstance;
            
            mediaEngineEx.SetSourceFromByteStream(new ByteStream(currentMusic.Stream), "MP3");
        }

        private struct MediaEngineErrorCodes
        {
            public long Parameter1;
            public int Parameter2;

            public MediaEngineErrorCodes(long param1, int param2)
            {
                Parameter1 = param1;
                Parameter2 = param2;
            }
        }

        private void OnMediaEngineEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            switch (mediaEvent)
            {
                case MediaEngineEvent.LoadedMetadata:
                    break;
                case MediaEngineEvent.CanPlay:
                    musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ReadyToBePlayed, null));
                    break;
                case MediaEngineEvent.Ended:
                    musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.EndOfTrackReached, null));
                    break;
                case MediaEngineEvent.Error:
                    musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ErrorOccurred, new MediaEngineErrorCodes(param1, param2)));
                    break;
            }
        }

        private void ProcessMusicError(SoundMusicEventNotification eventNotification)
        {
            if (eventNotification.Event == SoundMusicEvent.ErrorOccurred)
            {
                var errorCodes = (MediaEngineErrorCodes) eventNotification.EventData;

                if (errorCodes.Parameter1 == (long)MediaEngineErr.SourceNotSupported)
                {
                    if(currentMusic!=null)
                        ResetMusicPlayer();
                    else
                        throw new AudioSystemInternalException("Audio Engine is in an unconsistant state. CurrentMusic is null while Error on Unsupported media was reached.");

                    throw new InvalidOperationException("The data format of the source music file is not valid.");
                }

                throw new AudioSystemInternalException("An unhandled exception happened in media engine asynchronously. Error info [param1="+errorCodes.Parameter1+", param2="+errorCodes.Parameter2+"].");
            }
        }

        private void ProcessMusicMetaData()
        {
            throw new System.NotImplementedException();
        }

        private void ProcessPlayerClosed()
        {
            throw new System.NotImplementedException();
        }

        private void PlatformSpecificProcessMusicReady()
        {
            // nothing to do here
        }
    }
}

#endif