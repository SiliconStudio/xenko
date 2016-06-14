// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using AudioUnit;
using AVFoundation;

namespace SiliconStudio.Xenko.Audio
{
    public class AudioEngineIos : AudioEngine
    {
        public override void InitializeAudioEngine()
        {
            ActivateAudioSession();
            base.InitializeAudioEngine();
        }

        private void ActivateAudioSession()
        {
            const double preferedAudioLatency = 0.005;

            // start the AudioSession
            var audioSession = AVAudioSession.SharedInstance();

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

            //todo remove when we update libcore with no more audiounit stuff
            int err;
            var foo = AUGraph.Create(out err);
            foo.Dispose();
        }
    }
}
#endif
