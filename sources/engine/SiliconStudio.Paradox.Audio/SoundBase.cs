// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Audio
{
    /// <summary>
    /// Base class for all the sounds and sound instances.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract partial class SoundBase : ComponentBase
    {
        /// <summary>
        /// Create an instance of soundBase.
        /// </summary>
        /// <param name="engine">A valid AudioEngine</param>
        /// <exception cref="ArgumentNullException">The engine argument is null</exception>
        internal SoundBase(AudioEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            
            AudioEngine = engine;
        }

        internal AudioEngine AudioEngine { get; private set; }

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
