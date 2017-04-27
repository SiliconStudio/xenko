// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Audio
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
