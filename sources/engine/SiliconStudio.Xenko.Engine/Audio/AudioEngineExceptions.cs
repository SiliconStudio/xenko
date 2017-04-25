// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// The exception that is thrown when an internal error happened in the Audio System. That is an error that is not due to the user behavior.
    /// </summary>
    public class AudioEngineInternalExceptions : Exception
    {
        internal AudioEngineInternalExceptions(string msg)
            : base("An internal error happened in the audio engine [details:'" + msg + "'")
        { }
    }
}
