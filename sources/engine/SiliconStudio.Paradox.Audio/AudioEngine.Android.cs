// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Diagnostics;
using IOException = Java.IO.IOException;
using Android.Media;
using Java.IO;
using Java.Lang;

namespace SiliconStudio.Paradox.Audio
{
    partial class AudioEngine
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();

        private void DestroyImpl()
        {
            if (nbOfAudioEngineInstances == 0)
                SoundEffectInstance.StaticDestroy();

            mediaPlayer.Release();
        }

        private void AudioEngineImpl(AudioDevice device)
        {
            mediaPlayer.Completion += OnMusicCompletion;
            mediaPlayer.Error += OnMusicError;
            mediaPlayer.Prepared += OnMusicPrepared;

            if(nbOfAudioEngineInstances == 0)
                SoundEffectInstance.CreateAudioTracks();
        }

        private void StopCurrentMusic()
        {
            mediaPlayer.Stop();

            ResetMusicPlayer();
        }

        private void StartCurrentMusic()
        {
            if (isMusicPlayerReady)
                mediaPlayer.Start();
            else
                mediaPlayer.PrepareAsync();
        }

        private void PauseCurrentMusic()
        {
            mediaPlayer.Pause();
        }

        private void UpdateMusicVolume()
        {
            // volume factor used in order to adjust Sound Music and Sound Effect Maximum volumes
            const float volumeAdjustFactor = 0.5f;
            
            var vol = volumeAdjustFactor * currentMusic.Volume;

            mediaPlayer.SetVolume(vol, vol);
        }

        private void ResetMusicPlayer()
        {
            mediaPlayer.Reset();

            isMusicPlayerReady = false;

            currentMusic = null;
        }

        private Stopwatch loadTime = new Stopwatch();
        private void LoadNewMusic(SoundMusic soundMusic)
        {
            loadTime.Restart();
            try
            {
                using (var javaFileStream = new FileInputStream(soundMusic.FileName))
                    mediaPlayer.SetDataSource(javaFileStream.FD, soundMusic.StartPosition, soundMusic.Length);

                mediaPlayer.PrepareAsync();

                currentMusic = soundMusic;
            }
            catch (IOException)
            {
                // this can happen namely if too many files are already opened (should not throw an exception)
                Logger.Warning("The audio file '{0}' could not be opened", soundMusic.FileName);
            }
            catch (SecurityException)
            {
                throw new InvalidOperationException("The sound file is not accessible anymore.");
            }
            catch (IllegalArgumentException e)
            {
                throw new AudioSystemInternalException("Error during the SetDataSouce: "+e);
            }
        }

        private void OnMusicCompletion(object o, EventArgs args)
        {
            musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.EndOfTrackReached, args));
        }

        private void OnMusicError(object o, MediaPlayer.ErrorEventArgs args)
        {
            musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ErrorOccurred, args));
        }

        private void OnMusicPrepared(object o, EventArgs args)
        {
            musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ReadyToBePlayed, args));
        }

        private void RestartCurrentMusic()
        {
            mediaPlayer.SeekTo(0);
        }

        private void PlatformSpecificProcessMusicReady()
        {
            loadTime.Stop();

            if(currentMusic != null)
                Logger.Verbose("Time taken for music '{0}' loading = {0}", currentMusic.Name, loadTime.ElapsedMilliseconds);
        }

        private void ProcessPlayerClosed()
        {
            throw new AudioSystemInternalException("Should never arrive here. (Used only by windows implementation.");
        }

        private void ProcessMusicMetaData()
        {
            throw new AudioSystemInternalException("Should never arrive here. (Used only by windows implementation.");
        }

        private void ProcessMusicError(SoundMusicEventNotification eventNotification)
        {
            if (eventNotification.Event != SoundMusicEvent.ErrorOccurred) // no errors
                return;

            var soundMusicName = "Unknown";

            if (currentMusic != null)
            {
                currentMusic.SetStateToStopped();
                soundMusicName = currentMusic.Name;
            }

            Logger.Error("Error while playing the sound music '{0}'. Details follows:");

            var errorEvent = (MediaPlayer.ErrorEventArgs)eventNotification.EventData;
            if (errorEvent.What == MediaError.Unknown && errorEvent.Extra == 0xfffffc0e) // MEDIA_ERROR_UNSUPPORTED (Hardware dependent?)
            {
                Logger.Error("The data format of the music file is not supported.");
            }
            else if ((uint)errorEvent.What == 0xffffffed) // underlying audio track returned -12 (no memory) -> try to recreate the player once more
            {
                Logger.Error("The OS did not have enough memory to create the audio track.", soundMusicName);
            }
            else
            {
                Logger.Error(" [Details: ErrorCode={1}, Extra={2}]", soundMusicName, errorEvent.What, errorEvent.Extra);
            }

            // reset the music player to a valid state for future plays
            ResetMusicPlayer();
        }

        private void PauseAudioPlatformSpecific()
        {
            // nothing to do for windows
        }

        private void ResumeAudioPlatformSpecific()
        {
            // nothing to do for windows
        }
    }
}

#endif