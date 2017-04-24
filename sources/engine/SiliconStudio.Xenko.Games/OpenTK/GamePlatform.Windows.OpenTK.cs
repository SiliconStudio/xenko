// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF) && SILICONSTUDIO_XENKO_UI_OPENTK
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Games
{
    internal class GamePlatformOpenTK : GamePlatformWindows, IGraphicsDeviceFactory
    {
        public GamePlatformOpenTK(GameBase game) : base(game)
        {
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
