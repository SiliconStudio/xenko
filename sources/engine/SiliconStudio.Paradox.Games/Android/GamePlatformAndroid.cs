// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Games
{
    internal class GamePlatformAndroid : GamePlatform, IGraphicsDeviceFactory
    {
        public GamePlatformAndroid(GameBase game) : base(game)
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
            return new GameWindow[] { new GameWindowAndroid() };
        }

        public override List<GraphicsDeviceInformation> FindBestDevices(GameGraphicsParameters preferredParameters)
        {
            var gameWindowAndroid = gameWindow as GameWindowAndroid;
            if (gameWindowAndroid != null)
            {
                // Everything is already created at this point, just transmit what has been done
                var deviceInfo = new GraphicsDeviceInformation
                    {
                        Adapter = GraphicsAdapterFactory.Default,
                        GraphicsProfile = GraphicsProfile.Level_9_3,
                        PresentationParameters = new PresentationParameters(gameWindowAndroid.ClientBounds.Width,
                                                                            gameWindowAndroid.ClientBounds.Height,
                                                                            gameWindowAndroid.NativeWindow)
                            {
                                // TODO: PDX-364: Transmit what was actually created
                                BackBufferFormat = preferredParameters.PreferredBackBufferFormat,
                                DepthStencilFormat = preferredParameters.PreferredDepthStencilFormat,
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