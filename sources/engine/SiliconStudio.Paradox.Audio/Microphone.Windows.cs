// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Audio
{
    internal partial class Microphone
    {
        /// <summary>
        /// Create a new instance of Microphone ready for recording.
        /// </summary>
        /// <exception cref="NoMicrophoneConnectedException">No microphone is currently plugged.</exception>
        public Microphone()
        {
            throw new NotImplementedException();
        }

        #region Implementation of the IRecorder interface

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
