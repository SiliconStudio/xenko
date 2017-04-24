// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_WINDOWS || SILICONSTUDIO_PLATFORM_UNIX

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// Common ancestor to all game contexts on the Windows platform.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    public abstract class GameContextWindows<TK> : GameContext<TK>
    {
        /// <inheritDoc/>
        protected GameContextWindows(TK control, int requestedWidth = 0, int requestedHeight = 0)
            : base(control, requestedWidth, requestedHeight)
        {
        }
    }
}
#endif
