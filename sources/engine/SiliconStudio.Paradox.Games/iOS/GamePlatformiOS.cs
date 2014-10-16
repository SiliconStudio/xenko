// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Games
{
    internal class GamePlatformiOS : GamePlatform, IGraphicsDeviceFactory
    {
        public GamePlatformiOS(GameBase game) : base(game)
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
            return new GameWindow[] { new GameWindowiOS() };
        }

        public override List<GraphicsDeviceInformation> FindBestDevices(GameGraphicsParameters preferredParameters)
        {
            var gameWindowiOS = gameWindow as GameWindowiOS;
            if (gameWindowiOS != null)
            {
                // Unlike Desktop and WinRT, the list of best devices are completely fixed in WP8 XAML
                // So we return a single element
                var deviceInfo = new GraphicsDeviceInformation
                    {
                        Adapter = GraphicsAdapterFactory.Default,
                        GraphicsProfile = GraphicsProfile.Level_9_3,
                        PresentationParameters = new PresentationParameters(gameWindowiOS.ClientBounds.Width,
                                                                            gameWindowiOS.ClientBounds.Height,
                                                                            gameWindowiOS.NativeWindow)
                            {
                                DepthStencilFormat = PixelFormat.D16_UNorm,
                            }
                    }; 

                return new List<GraphicsDeviceInformation>() { deviceInfo };
            }
            return base.FindBestDevices(preferredParameters);
        }

        public override void DeviceChanged(GraphicsDevice currentDevice, GraphicsDeviceInformation deviceInformation)
        {
            // TODO: Check when it needs to be disabled on iOS (OpenGL)?
            // Force to resize the gameWindow
            //gameWindow.Resize(deviceInformation.PresentationParameters.BackBufferWidth, deviceInformation.PresentationParameters.BackBufferHeight);
        }
    }
}
#endif