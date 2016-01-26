// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    public static class InputManagerFactory
    {
        /// <summary>
        /// Create the appropriate instance of InputManager depending on the platform and context associated to <paramref name="registry"/>.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="context">Associated context. Cannot be null.</param>
        /// <returns></returns>
        public static InputManager NewInputManager(IServiceRegistry registry, GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            InputManager res = null;

            switch (context.ContextType)
            {
                case AppContextType.Desktop:
#if SILICONSTUDIO_XENKO_UI_WINFORMS
                    res = NewInputManagerWinforms(registry);
#elif SILICONSTUDIO_XENKO_UI_SDL
                    // When SDL is the only UI available, Desktop and DesktopSDL are equivalent.
                    res = NewInputManagerSDL(registry);
#endif
                    break;

                case AppContextType.DesktopWpf:
                    res = NewInputManagerWpf(registry);
                    break;

                case AppContextType.DesktopOpenTK:
                    res = NewInputManagerOpenTK(registry);
                    break;

                case AppContextType.DesktopSDL:
                    res = NewInputManagerSDL(registry);
                    break;

                case AppContextType.WindowsRuntime:
                    res = NewInputManagerWindowsRuntime(registry);
                    break;

                case AppContextType.Android:
                    res = NewInputManagerAndroid(registry);
                    break;

                case AppContextType.iOS:
                    res = NewInputManageriOS(registry);
                    break;
            }
            if (res == null)
            {
                throw new NotSupportedException("Unsupported Input type");
            }
            return res;
        }

        private static InputManager NewInputManagerWinforms(IServiceRegistry registry)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_UI_WINFORMS
            return new InputManagerWinforms(registry);
#else
            return null;
#endif
        }

        private static InputManager NewInputManagerWpf(IServiceRegistry registry)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_UI_WPF && !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
            return new InputManagerWpf(registry);
#else
            return null;
#endif
        }

        private static InputManager NewInputManagerSDL(IServiceRegistry registry)
        {
#if SILICONSTUDIO_XENKO_UI_SDL
            return new InputManagerSDL(registry);
#else
            return null;
#endif
        }

        private static InputManager NewInputManagerOpenTK(IServiceRegistry registry)
        {
#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK
            return new InputManagerOpenTK(registry);
#else
            return null;
#endif
        }

        private static InputManager NewInputManagerWindowsRuntime(IServiceRegistry registry)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
            return new InputManagerWindowsRuntime(registry);
#else
            return null;
#endif
        }

        private static InputManager NewInputManagerAndroid(IServiceRegistry registry)
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            return new InputManagerAndroid(registry);
#else
            return null;
#endif
        }

        private static InputManager NewInputManageriOS(IServiceRegistry registry)
        {
#if SILICONSTUDIO_PLATFORM_IOS
            return new InputManageriOS(registry);
#else
            return null;
#endif
        }
    }
}
