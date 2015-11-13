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
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
#if !SILICONSTUDIO_UI_SDL_ONLY
                case AppContextType.Desktop:
                    res = new InputManagerWinforms(registry);
                    break;
                case AppContextType.DesktopWpf:
#if !SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
                    res = new InputManagerWpf(registry);
#endif
                    break;
#else
                case AppContextType.Desktop:
                    res = new InputManagerSDL(registry);
                    break;
#endif
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
                case AppContextType.DesktopOpenTK:
                    res = new InputManagerOpenTK(registry);
                    break;
#endif
                case AppContextType.DesktopSDL:
                    res = new InputManagerSDL(registry);
                    break;
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
                case AppContextType.WindowsRuntime:
                    res = new InputManagerWindowsRuntime(registry);
                    break;
#endif
#else
#if SILICONSTUDIO_PLATFORM_ANDROID
                case AppContextType.Android:
                    res = new InputManagerAndroid(registry);
                    break;
#endif
#if SILICONSTUDIO_PLATFORM_IOS
                case AppContextType.iOS:
                    res = new InputManageriOS(registry);
                    break;
#endif
#endif
            }
            if (res == null)
            {
                throw new NotSupportedException("Unsupported Input type");
            }
            return res;
        }
    }
}
