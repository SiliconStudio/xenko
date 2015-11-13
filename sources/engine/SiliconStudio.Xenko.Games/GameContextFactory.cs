// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Games
{
    public static class GameContextFactory
    {
        /// <summary>
        /// Based on the compilation flags, create the appropriate context.
        /// </summary>
        /// <returns></returns>
        public static GameContext NewGameContext()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
    #if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            return new GameContextOpenTk(null);
    #else
        #if SILICONSTUDIO_UI_SDL_ONLY
            return new GameContextSdl(null);
        #else
            // In theory we could choose between GameContextWinforms and GameContextSdl,
            // but we would need an argument to NewGameContext and update the callers
            // accordingly.
            return new GameContextWinforms(null);
        #endif
    #endif
#endif
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            return new GameContextWindowsRuntime(null);
#endif
#if SILICONSTUDIO_PLATFORM_ANDROID
            return new GameContextAndroid(null, null);
#endif
#if SILICONSTUDIO_PLATFORM_IOS
            return new GameContextiOS(null, null, null, 0, 0);
#endif
        }
    }
}
