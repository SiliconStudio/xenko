// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_SOUND_SDL

using System;

namespace SiliconStudio.Xenko.Audio
{
    public partial class DynamicSoundEffectInstance
    {
        private void SubmitBufferImpl(byte[] buffer, int offset, int byteCount)
        {
            throw new NotImplementedException();
        }

        private void InitializeDynamicSound()
        {
            throw new NotImplementedException();
        }

        private void ClearBuffersImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A list of all the data handles lock for one subBuffer.
        /// </summary>
        private class SubBufferDataHandles
        {
            public void FreeHandles()
            {
                HandleCount = 0;
            }

            public int HandleCount { get; private set; }

            public void AddHandle()
            {
                HandleCount = HandleCount + 1;
            }
        };
    }
}

#endif