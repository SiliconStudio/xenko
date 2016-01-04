// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using System;
using AVFoundation;
using AudioToolbox;
using Foundation;

namespace SiliconStudio.Xenko.Audio
{
    public class AudioEngineiOS: AudioEngine
    {
        private AVAudioPlayer audioPlayer;
        private bool currentMusicDataTypeIsUnsupported;

        internal override void InitializeAudioEngine(AudioDevice device)
        {
            if (nbOfAudioEngineInstances == 0)
                ActivateAudioSession();
        }

        private void ActivateAudioSession()
        {
            NSError error;
            const double preferedAudioLatency = 0.005;

            // start the AudioSession
            var audioSession = AVAudioSession.SharedInstance();

            // set the audio category so that: playback/recording is possible, playback is mixed with background music, music is stopped when screen is locked
            error = audioSession.SetCategory(AVAudioSessionCategory.SoloAmbient);
            if (error != null)
            {
                Logger.Warning("Failed to set the audio category to 'Ambient'. [Error info: {0}]", error.UserInfo);
                State = AudioEngineState.Invalidated;
                return;
            }

            // Reduce the buffer size for better latency response..
            audioSession.SetPreferredIOBufferDuration(preferedAudioLatency, out error);
            if (error != null)
                Logger.Warning("Failed to set the audio IO buffer duration to '{1}'. [Error info: {0}]", error.UserInfo,
                            preferedAudioLatency);

            // set the preferred sampling rate of the application
            if (AudioSampleRate != 0)
            {
                audioSession.SetPreferredSampleRate(AudioSampleRate, out error);
                Logger.Warning("Failed to set the audio session preferred sampling rate to '{1}'. [Error info: {0}]",
                            error.UserInfo, AudioSampleRate);
            }

            // activate the sound for the application
            error = audioSession.SetActive(true);
            if (error != null)
            {
                Logger.Warning("Failed to activate the audio session. [Error info: {0}]", error.UserInfo);
                State = AudioEngineState.Invalidated;
            }
        }

        internal override void DestroyAudioEngine()
        {
            ResetMusicPlayer();
        }

        internal override void LoadNewMusic(SoundMusic lastPlayRequestMusicInstance)
        {
            if(audioPlayer != null)
                throw new AudioSystemInternalException("Tried to create a new AudioPlayer but the current instance was not freed.");

            currentMusic = lastPlayRequestMusicInstance;

            currentMusicDataTypeIsUnsupported = false;

            NSError loadError;

            // TODO: Avoid allocating twice the music size (i.e. by using NSData.FromBytesNoCopy on currentMusic.Stream.GetBuffer())
            currentMusic.Stream.Position = 0;
            audioPlayer = AVAudioPlayer.FromData(NSData.FromStream(currentMusic.Stream), out loadError);

            if (loadError != null)
            {
                if (loadError.Code == (int) AudioFileError.UnsupportedFileType || loadError.Code == (int) AudioFileError.UnsupportedDataFormat)
                {
                    currentMusicDataTypeIsUnsupported = true;
                    musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.MetaDataLoaded, null));
                    return;
                }
                throw new AudioSystemInternalException("Music loading failed and failure was not handled. [Error="+loadError.LocalizedDescription+"].");
            }

            if (audioPlayer == null) // both audioPlayer and loadError are null (happened before when url was not correct) 
                throw new AudioSystemInternalException("Music loading failed and failure was not handled. [Unspecified Error].");

            audioPlayer.DecoderError += OnAudioPlayerDecoderError;
            audioPlayer.FinishedPlaying += OnAudioPlayerFinishedPlaying;

            if (!audioPlayer.PrepareToPlay())
            {
                // this happens sometimes when we put the application on background when starting to play.
                var currentMusicName = currentMusic.Name;
                currentMusic.SetStateToStopped();
                ResetMusicPlayer();

                Logger.Warning("The music '{0}' failed to prepare to play.", currentMusicName);
            }
            else
            {
                musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.MetaDataLoaded, null));
                musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ReadyToBePlayed, null));
            }
        }

        private void OnAudioPlayerFinishedPlaying(object sender, AVStatusEventArgs e)
        {
            if(!e.Status)
                throw new AudioSystemInternalException("The music play back did not completed successfully.");

            musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.EndOfTrackReached, e));
        }

        private void OnAudioPlayerDecoderError(object sender, AVErrorEventArgs e)
        {
            musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ErrorOccurred, e));
        }

        internal override void ResetMusicPlayer()
        {
            currentMusic = null;

            if (audioPlayer != null)
            {
                audioPlayer.Dispose();
                audioPlayer = null;
            }
        }

        internal override void StopMusic()
        {
            if(audioPlayer == null)
                return;

            audioPlayer.Stop();
            audioPlayer.CurrentTime = 0;
        }

        internal override void PauseMusic()
        {
            if (audioPlayer == null)
                return;

            audioPlayer.Pause();
        }

        internal override void UpdateMusicVolume()
        {
            if (audioPlayer == null)
                return;

            audioPlayer.Volume = currentMusic.Volume;
        }

        internal override void StartMusic()
        {
            if (audioPlayer == null)
                return;

            if (!audioPlayer.Play())
            {
                // this happens sometimes when we put the application on background when starting to play.
                var currentMusicName = currentMusic.Name;
                currentMusic.SetStateToStopped();
                ResetMusicPlayer();

                Logger.Warning("The music '{0}' failed to start playing.", currentMusicName);
            }
        }

        internal override void RestartMusic()
        {
            StopMusic();
            StartMusic();
        }

        internal override void ProcessMusicError(SoundMusicEventNotification eventNotification)
        {
            if (eventNotification.Event == SoundMusicEvent.ErrorOccurred)
            {
                var errorEventArgs = (AVErrorEventArgs) eventNotification.EventData;
                throw new AudioSystemInternalException("An error happened during music play back and was not handled by the AudioSystem. [error:" + errorEventArgs.Error.LocalizedDescription + "].");
            }
        }

        internal override void ProcessMusicMetaData()
        {
            var errorMsg = "Try to play a music with other format than PCM or MP3.";
            var errorInFormat = currentMusicDataTypeIsUnsupported;

            if (audioPlayer != null)
            {
                var settings = audioPlayer.SoundSetting;

                if (settings.NumberChannels > 2)
                {
                    errorInFormat = true;
                    errorMsg = "Try to play a music with more than two audio channels.";
                }
            }
            if (errorInFormat)
            {
                ResetMusicPlayer();
                throw new InvalidOperationException(errorMsg);
            }
        }

        internal override void ProcessPlayerClosed()
        {
            throw new AudioSystemInternalException("Should never arrive here. (Used only by windows implementation.");
        }

        /// <inheritDoc/>
        protected override void PauseAudioImpl()
        {
            // todo: Should we Inactivate the audio session here?
        }

        /// <inheritDoc/>
        protected override void ResumeAudioImpl()
        {
            ActivateAudioSession();
        }
    }
}
#endif
