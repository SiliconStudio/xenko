// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for all the sounds and sound instances.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract partial class SoundBase : ComponentBase
    {
        /// <summary>
        /// Create an empty instance of soundBase. Used for serialization.
        /// </summary>
        internal SoundBase()
        {
        }

        internal AudioEngineState EngineState { get { return AudioEngine.State; } }
        
        #region Disposing Utilities
        
        internal void CheckNotDisposed()
        {
            if(IsDisposed)
                throw new ObjectDisposedException("this");
        }

        #endregion
    }
}
