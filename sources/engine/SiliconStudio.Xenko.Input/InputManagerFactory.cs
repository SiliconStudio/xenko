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
        /// <returns></returns>
        public static InputManager NewInputManager(IServiceRegistry registry, GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            switch (context.ContextType)
            {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
#if !SILICONSTUDIO_UI_SDL_ONLY
                case AppContextType.Desktop:
                case AppContextType.DesktopWpf:
                    return new InputManagerWinforms(registry);
#else
                case AppContextType.Desktop:
                    return new InputManagerSDL(registry);
#endif
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
                case AppContextType.DesktopOpenTK:
                    return new InputManagerOpenTK(registry);
#endif
                case AppContextType.DesktopSDL:
                    return new InputManagerSDL(registry);
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
                case AppContextType.WindowsRuntime:
                    return new InputManagerWindowsRuntime(registry);
#endif
#else
#if SILICONSTUDIO_PLATFORM_ANDROID
                case AppContextType.Android:
                    return new InputManagerAndroid(registry);
#endif
#if SILICONSTUDIO_PLATFORM_IOS
                case AppContextType.iOS:
                    return new InputManageriOS(registry);
#endif
#endif
                default:
                    throw new NotSupportedException("Unsupported Input type");
            }
        }
    }
}
