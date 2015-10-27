// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Regression
{
    public class TestGameBase : Game
    {
        public static readonly Logger TestGameLogger = GlobalLogger.GetLogger("TestGameLogger");

        public int StopOnFrameCount { get; set; }

        public TestGameBase()
        {
            // Enable profiling
            //Profiler.EnableAll();
            GraphicsDeviceManager.PreferredBackBufferWidth = 800;
            GraphicsDeviceManager.PreferredBackBufferHeight = 480;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            StopOnFrameCount = -1;
            AutoLoadDefaultSettings = false;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.FrameCount == StopOnFrameCount)
            {
                Exit();
            }
        }
    }
}