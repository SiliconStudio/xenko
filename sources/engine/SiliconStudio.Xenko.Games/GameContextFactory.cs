// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// Given a <see cref="AppContextType"/> creates the corresponding GameContext instance based on the current executing platform.
    /// </summary>
    public static class GameContextFactory
    {

        [Obsolete("Use NewGameContext with the proper AppContextType.")]
        internal static GameContext NewDefaultGameContext()
        {
            // Default context is Desktop
            AppContextType type = AppContextType.Desktop;
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
    #if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            type = AppContextType.DesktopOpenTK;
    #else
        #if SILICONSTUDIO_UI_SDL_ONLY
            type = AppContextType.DesktopSDL;
        #else
            type = AppContextType.Desktop;
        #endif
    #endif
#endif
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            type = AppContextType.WindowsRuntime;
#endif
#if SILICONSTUDIO_PLATFORM_ANDROID
            type = AppContextType.Android;
#endif
#if SILICONSTUDIO_PLATFORM_IOS
            type = AppContextType.iOS;
#endif
            return NewGameContext(type);
        }

        /// <summary>
        /// Given a <paramref name="type"/> create the appropriate game Context for the current executing platform.
        /// </summary>
        /// <returns></returns>
        public static GameContext NewGameContext(AppContextType type)
        {
            GameContext res = null;
            switch (type)
            {
                case AppContextType.Android:
                    res = NewGameContextAndroid();
                    break;
                case AppContextType.Desktop:
                    res = NewGameContextDesktop();
                    break;
                case AppContextType.DesktopOpenTK:
                    res = NewGameContextOpenTk();
                    break;
                case AppContextType.DesktopSDL:
                    res = NewGameContextSdl();
                    break;
                case AppContextType.DesktopWpf:
                    res = NewGameContextWpf();
                    break;
                case AppContextType.WindowsRuntime:
                    res = NewGameContextWindowsRuntime();
                    break;
                case AppContextType.iOS:
                    res = NewGameContextiOS();
                    break;
            }

            if (res == null)
            {
                throw new InvalidOperationException("Requested type and current platform are incompatible.");
            }

            return res;
        }

        public static GameContext NewGameContextiOS()
        {
#if SILICONSTUDIO_PLATFORM_IOS
            return new GameContextiOS(new iOSWindow(null, null, null), 0, 0);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextAndroid()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            return new GameContextAndroid(null, null);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextDesktop()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
    #if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            return new GameContextOpenTk(null);
    #else
        #if !SILICONSTUDIO_UI_SDL_ONLY
            return new GameContextWinforms(null);
        #else
            return new GameContextSdl(null);
        #endif
    #endif
#else
            return null;
#endif
        }

        public static GameContext NewGameContextWindowsRuntime()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            return new GameContextWindowsRuntime(null);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextOpenTk()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            return new GameContextOpenTk(null);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextSdl()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_UI_SDL_ONLY
            return new GameContextSdl(null);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextWpf()
        {
            // Not supported for now.
            return null;
        }
    
    }
}
