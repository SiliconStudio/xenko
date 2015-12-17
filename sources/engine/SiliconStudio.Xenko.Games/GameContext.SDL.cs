// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_UI_SDL
using SiliconStudio.Xenko.Graphics.SDL;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing SDL Window.
    /// </summary>
    public class GameContextSDL : GameContextWindows<Window>
    {
        /// <inheritDoc/>
        public GameContextSDL(Window control, int requestedWidth = 0, int requestedHeight = 0)
            : base(control ?? new GameFormSDL(), requestedWidth, requestedHeight) 
        {
            ContextType = AppContextType.DesktopSDL;
        }
    }
}
#endif
