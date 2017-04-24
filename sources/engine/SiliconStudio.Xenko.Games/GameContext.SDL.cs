// Copyright (c) 2015-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_UI_SDL
using SiliconStudio.Xenko.Graphics.SDL;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// A <see cref="GameContext"/> to use for rendering to an existing SDL Window.
    /// </summary>
    public class GameContextSDL : GameContextWindows<Window>
    {
        static GameContextSDL()
        {
            // Preload proper SDL native library (depending on CPU type)
            Core.NativeLibrary.PreloadLibrary("SDL2.dll");
        }

        /// <inheritDoc/>
        public GameContextSDL(Window control, int requestedWidth = 0, int requestedHeight = 0)
            : base(control ?? new GameFormSDL(), requestedWidth, requestedHeight) 
        {
            ContextType = AppContextType.DesktopSDL;
        }
    }
}
#endif
