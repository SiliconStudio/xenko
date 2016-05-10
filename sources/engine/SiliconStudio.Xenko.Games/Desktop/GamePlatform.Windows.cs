// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX
using System;
using System.IO;
using System.Reflection;

namespace SiliconStudio.Xenko.Games
{
    internal class GamePlatformWindows : GamePlatform
    {
        public GamePlatformWindows(GameBase game) : base(game)
        {
            IsBlockingRun = true;
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_RUNTIME_CORECLR
                // This is required by the Audio subsystem of SharpDX.
            Win32Native.CoInitialize(IntPtr.Zero);
#endif
        }

        public override string DefaultAppDirectory
        {
            get
            {
                var assemblyUri = new Uri(Assembly.GetEntryAssembly().CodeBase);
                return Path.GetDirectoryName(assemblyUri.LocalPath);
            }
        }

        internal override GameWindow GetSupportedGameWindow(AppContextType type)
        {
            switch (type)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK
                case AppContextType.DesktopOpenTK:
                    return new GameWindowOpenTK();
#endif

#if SILICONSTUDIO_XENKO_UI_SDL
                 case AppContextType.DesktopSDL:
                    return new GameWindowSDL();
#endif

                 case AppContextType.Desktop:
#if (SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D || SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN) && SILICONSTUDIO_XENKO_UI_WINFORMS
                    return new GameWindowWinforms();
#elif SILICONSTUDIO_XENKO_UI_SDL
                    return new GameWindowSDL();
#else
                    return null;
#endif

#if SILICONSTUDIO_XENKO_UI_WPF
                case AppContextType.DesktopWpf:
                    // WPF is not supported yet.
                    return null;
#endif

                default:
                    return null;
            }
        }
    }
}
#endif
