// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using SiliconStudio.Xenko.Audio.Wave;

namespace SiliconStudio.Xenko.Audio
{
    public partial class DynamicSoundEffectInstance
    {
        private void ClearBuffersImpl()
        {
            throw new System.NotImplementedException();
        }

        private void SubmitBufferImpl(byte[] buffer, int offset, int byteCount)
        {
            throw new System.NotImplementedException();
        }

        private void InitializeDynamicSound()
        {
            throw new System.NotImplementedException();
        }

        private void CreateVoice(WaveFormat waveFormat1)
        {
            throw new System.NotImplementedException();
        }

        private class SubBufferDataHandles
        {
            public void FreeHandles()
            {
                throw new System.NotImplementedException();
            }

            public int HandleCount
            {
                get { throw new System.NotImplementedException(); }
                set { throw new System.NotImplementedException(); }
            }
        }
    }
}

#endif