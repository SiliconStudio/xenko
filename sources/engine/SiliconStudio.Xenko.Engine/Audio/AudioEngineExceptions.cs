// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Audio
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
