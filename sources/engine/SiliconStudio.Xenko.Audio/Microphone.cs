// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Class implementing the IRecoder interface designed to capture microphone audio input. Refer to <see cref="IRecorder"/> for more details.
    /// </summary>
    internal sealed partial class Microphone : IRecorder
    {
        #region Implementation of the IRecorder interface

        public TimeSpan BufferDuration { get; set; }

        public TimeSpan BufferSize { get; set; }

        public int SampleRate { get; private set; }

        public RecorderState State { get; private set; }

        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            throw new NotImplementedException();
        }

        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public int GetData(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public int GetData(byte[] buffer, int offset, int count)
        {
            // Just to avoid warning on BufferReady, code to remove
            if (BufferReady != null) BufferReady(this, EventArgs.Empty);
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> BufferReady;

        #endregion
    }
}
