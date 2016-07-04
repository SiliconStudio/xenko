// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using AVFoundation;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    public class AudioEngineIos : AudioEngine
    {
        internal override void InitializeAudioEngine()
        {
            ActivateAudioSession();
            base.InitializeAudioEngine();
        }

        private void ActivateAudioSession()
        {
            const double preferedAudioLatency = 0.005;

            // start the AudioSession
            var audioSession = AVAudioSession.SharedInstance();

            AVAudioSession.Notifications.ObserveInterruption((sender, args) =>
            {
                if (args.InterruptionType == AVAudioSessionInterruptionType.Began)
                {
                    AudioLayer.ListenerDisable(DefaultListener.Listener);
                }
                else
                {
                    AudioLayer.ListenerEnable(DefaultListener.Listener);
                }
            });

            // set the audio category so that: playback/recording is possible, playback is mixed with background music, music is stopped when screen is locked
            var error = audioSession.SetCategory(AVAudioSessionCategory.SoloAmbient);
            if (error != null)
            {
                Logger.Warning("Failed to set the audio category to 'Ambient'. [Error info: {0}]", error.UserInfo);
                State = AudioEngineState.Invalidated;
                return;
            }

            // Reduce the buffer size for better latency response..
            audioSession.SetPreferredIOBufferDuration(preferedAudioLatency, out error);
            if (error != null)
                Logger.Warning("Failed to set the audio IO buffer duration to '{1}'. [Error info: {0}]", error.UserInfo, preferedAudioLatency);

            // set the preferred sampling rate of the application
            if (AudioSampleRate != 0)
            {
                audioSession.SetPreferredSampleRate(AudioSampleRate, out error);
                Logger.Warning("Failed to set the audio session preferred sampling rate to '{1}'. [Error info: {0}]", error.UserInfo, AudioSampleRate);
            }

            // activate the sound for the application
            error = audioSession.SetActive(true);
            if (error != null)
            {
                Logger.Warning("Failed to activate the audio session. [Error info: {0}]", error.UserInfo);
                State = AudioEngineState.Invalidated;
            }
        }
    }
}
#endif
