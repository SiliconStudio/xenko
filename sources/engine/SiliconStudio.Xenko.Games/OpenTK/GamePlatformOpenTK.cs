// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && !SILICONSTUDIO_UI_SDL2
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Games
{
    internal class GamePlatformOpenTK : GamePlatform, IGraphicsDeviceFactory
    {
        public GamePlatformOpenTK(GameBase game) : base(game)
        {
        }

        public override string DefaultAppDirectory
        {
            get
            {
                var assemblyUri = new Uri(Assembly.GetEntryAssembly().CodeBase);
                return Path.GetDirectoryName(assemblyUri.LocalPath);
            }
        }

        internal override GameWindow[] GetSupportedGameWindows()
        {
            return new GameWindow[] { new GameWindowOpenTK() };
        }

        public virtual void DeviceChanged(GraphicsDevice currentDevice, GraphicsDeviceInformation deviceInformation)
        {
            // TODO: Check when it needs to be disabled on iOS (OpenGL)?
            // Force to resize the gameWindow
            //gameWindow.Resize(deviceInformation.PresentationParameters.BackBufferWidth, deviceInformation.PresentationParameters.BackBufferHeight);
        }
    }
}
#endif