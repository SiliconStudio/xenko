// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Diagnostics;
using IOException = Java.IO.IOException;
using Android.Media;
using Java.IO;
using Java.Lang;

namespace SiliconStudio.Xenko.Audio
{
    partial class AudioEngineAndroid: AudioEngine
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();

        internal override void DestroyAudioEngine()
        {
            if (nbOfAudioEngineInstances == 0)
                SoundEffectInstance.StaticDestroy();

            mediaPlayer.Release();
        }

        internal override void InitializeAudioEngine(AudioDevice device)
        {
            mediaPlayer.Completion += OnMusicCompletion;
            mediaPlayer.Error += OnMusicError;
            mediaPlayer.Prepared += OnMusicPrepared;

            if(nbOfAudioEngineInstances == 0)
                SoundEffectInstance.CreateAudioTracks();
        }

        internal override void StopMusic()
        {
            mediaPlayer.Stop();

            ResetMusicPlayer();
        }

        internal override void StartMusic()
        {
            if (isMusicPlayerReady)
                mediaPlayer.Start();
            else
                mediaPlayer.PrepareAsync();
        }

        internal override void PauseMusic()
        {
            mediaPlayer.Pause();
        }

        internal override void UpdateMusicVolume()
        {
            // volume factor used in order to adjust Sound Music and Sound Effect Maximum volumes
            const float volumeAdjustFactor = 0.5f;
            
            var vol = volumeAdjustFactor * CurrentMusic.Volume;

            mediaPlayer.SetVolume(vol, vol);
        }

        internal override void ResetMusicPlayer()
        {
            mediaPlayer.Reset();

            isMusicPlayerReady = false;

            CurrentMusic = null;
        }

        private Stopwatch loadTime = new Stopwatch();
        internal override void LoadMusic(SoundMusic music)
        {
            loadTime.Restart();
            try
            {
                using (var javaFileStream = new FileInputStream(music.FileName))
                    mediaPlayer.SetDataSource(javaFileStream.FD, music.StartPosition, music.Length);

                mediaPlayer.PrepareAsync();

                CurrentMusic = music;
            }
            catch (IOException)
            {
                // this can happen namely if too many files are already opened (should not throw an exception)
                Logger.Warning("The audio file '{0}' could not be opened", music.FileName);
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

        internal override void RestartMusic()
        {
            mediaPlayer.SeekTo(0);
        }

        internal override void ProcessMusicReadyImpl()
        {
            loadTime.Stop();

            if(CurrentMusic != null)
                Logger.Verbose("Time taken for music '{0}' loading = {0}", CurrentMusic.Name, loadTime.ElapsedMilliseconds);
        }

        internal override void ProcessPlayerClosed()
        {
            throw new AudioSystemInternalException("Should never arrive here. (Used only by windows implementation.");
        }

        internal override void ProcessMusicMetaData()
        {
            throw new AudioSystemInternalException("Should never arrive here. (Used only by windows implementation.");
        }

        internal override void ProcessMusicError(SoundMusicEventNotification eventNotification)
        {
            if (eventNotification.Event != SoundMusicEvent.ErrorOccurred) // no errors
                return;

            var soundMusicName = "Unknown";

            if (CurrentMusic != null)
            {
                CurrentMusic.SetStateToStopped();
                soundMusicName = CurrentMusic.Name;
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
    }
}

#endif
