// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

using System;

using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;

namespace SiliconStudio.Xenko.Audio
{
    public abstract class AudioEngineWindows : AudioEngine
    {
        internal XAudio2 XAudio2;

        internal X3DAudio X3DAudio;

        internal MasteringVoice MasteringVoice;

        private static bool mediaEngineStarted;

        #region Implementation of the IDisposable Interface

        internal override void DestroyAudioEngine()
        {
            if (MasteringVoice != null)
            {
                MasteringVoice.Dispose();
                MasteringVoice = null;
            }

            if (XAudio2 != null)
            {
                XAudio2.StopEngine();
                XAudio2.Dispose();
                XAudio2 = null;
            }

            DisposeImpl();

            if (mediaEngineStarted && nbOfAudioEngineInstances == 0)
            {
                MediaManager.Shutdown();
                mediaEngineStarted = false;
            }
        }

        /// <summary>
        /// Windows specific implementation of Dispose
        /// </summary>
        internal abstract void DisposeImpl();

        #endregion

        #region Windows abstractions
        internal abstract void InitImpl();

        #endregion

        #region Audio Hardware Selection

        // This region is currently nor implemented nor exposed to the client

        internal override void InitializeAudioEngine(AudioDevice device)
        {
            try
            {
                XAudio2 = new XAudio2();
                X3DAudio = new X3DAudio(Speakers.Stereo); // only stereo mode currently supported

                XAudio2.CriticalError += XAudio2OnCriticalError;

                MasteringVoice = new MasteringVoice(XAudio2, 2, (int)AudioSampleRate); // Let XAudio choose an adequate sampling rate for the platform and the configuration but not number of channels [force to stereo 2-channels]. 

                if (!mediaEngineStarted)
                {
                    // MediaManager.Startup(); <- MediaManager.Shutdown is buggy (do not set isStartUp to false) so we are forced to directly use MediaFactory.Startup here while sharpDX is not corrected.
                    MediaFactory.Startup(0x20070, 0);
                    mediaEngineStarted = true;
                }

                InitImpl();
            }
            catch (DllNotFoundException exp)
            {
                State = AudioEngineState.Invalidated;
                Logger.Warning("One or more of the XAudio and MediaFoundation dlls were not found on the computer. " +
                               "Audio functionalities will not fully work for the current game instance." +
                               "To fix the problem install or repair the installation of XAudio and Media Foundation. [Exception details: {0}]", exp.Message);
            }
            catch (SharpDX.SharpDXException exp)
            {
                State = AudioEngineState.Invalidated;

                if (exp.ResultCode == XAudioErrorCodes.ErrorInvalidCall)
                {
                    Logger.Warning("Initialization of the audio engine failed. This may be due to missing audio hardware or missing audio outputs. [Exception details: {0}]", exp.Message);
                }
                else if (exp.ResultCode == 0x8007007E)
                {
                    Logger.Warning( "Audio engine initialization failed. This is probably due to missing dll on your computer. " +
                                    "Please check that XAudio2 and MediaFoundation are correctly installed.[Exception details: {0}]", exp.Message);
                }
                else
                {
                    Logger.Warning("Audio engine initialization failed. [Exception details: {0}]", exp.Message);
                }
            }
        }

        private void XAudio2OnCriticalError(object sender, ErrorEventArgs errorEventArgs)
        {
            Logger.Error("XAudio2 critical error {0} ", errorEventArgs.ErrorCode);
        }

        #endregion
    }
}

#endif
