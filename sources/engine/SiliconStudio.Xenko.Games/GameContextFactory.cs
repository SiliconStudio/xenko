// Copyright (c) 2015-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX
    #if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
        #if SILICONSTUDIO_XENKO_UI_SDL
            type = AppContextType.DesktopSDL;
        #elif SILICONSTUDIO_XENKO_UI_OPENTK
            type = AppContextType.DesktopOpenTK;
        #endif
    #elif SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
        #if SILICONSTUDIO_XENKO_UI_SDL && !SILICONSTUDIO_XENKO_UI_WINFORMS && !SILICONSTUDIO_XENKO_UI_WPF
            type = AppContextType.DesktopSDL;
        #endif
    #else
            type = AppContextType.Desktop;
    #endif
#elif SILICONSTUDIO_PLATFORM_UWP
            type = AppContextType.UWPSwapChain; // Can change later to CoreWindow
#elif SILICONSTUDIO_PLATFORM_ANDROID
            type = AppContextType.Android;
#elif SILICONSTUDIO_PLATFORM_IOS
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
                    res = NewGameContextOpenTK();
                    break;
                case AppContextType.DesktopSDL:
                    res = NewGameContextSDL();
                    break;
                case AppContextType.DesktopWpf:
                    res = NewGameContextWpf();
                    break;
                case AppContextType.UWPSwapChain:
                case AppContextType.UWPCoreWindow:
                    res = NewGameContextUWP();
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
    #if SILICONSTUDIO_XENKO_UI_OPENTK
            return new GameContextOpenTK(null);
    #else
        #if SILICONSTUDIO_XENKO_UI_SDL && !SILICONSTUDIO_XENKO_UI_WINFORMS && !SILICONSTUDIO_XENKO_UI_WPF
            return new GameContextSDL(null);
        #elif (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
            return new GameContextWinforms(null);
        #else
            return null;
        #endif
    #endif
#else
            return null;
#endif
        }

        public static GameContext NewGameContextUWP()
        {
#if SILICONSTUDIO_PLATFORM_UWP
            return new GameContextUWP(null);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextOpenTK()
        {
#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK
            return new GameContextOpenTK(null);
#else
            return null;
#endif
        }

        public static GameContext NewGameContextSDL()
        {
#if SILICONSTUDIO_XENKO_UI_SDL
            return new GameContextSDL(null);
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
