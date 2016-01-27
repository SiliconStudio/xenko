// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;


namespace SiliconStudio.Xenko.Particles.Tests
{
    /// <summary>
    /// Game class for test on the Input sensors.
    /// </summary>
    class GameTest : GameTestBase
    {
        // Local screenshots
        private readonly string xenkoDir;
        private readonly string assemblyName;
        private readonly string testName;
        private readonly string platformName;
        private int screenShots;


        public GameTest(string name)
        {
            screenShots = 0;
            testName = name;
            xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");
//            var gamePath = "Particles";
//            assemblyName = Path.GetFileNameWithoutExtension(gamePath);
            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            //  SaveScreenshot is only defined for windows
            platformName = "Windows";
            Directory.CreateDirectory(xenkoDir + "\\screenshots\\");
#endif

            CurrentVersion = 1;
            AutoLoadDefaultSettings = true;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1, };
            
            IsFixedTimeStep = true;
           // This still doesn't work IsDrawDesynchronized = false; // Double negation!
            TargetElapsedTime = TimeSpan.FromTicks(10000000 / 60); // target elapsed time is by default 60Hz
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot(60);

            //FrameGameSystem.Draw(EmptyDraw).TakeScreenshot();
        }

        private void EmptyDraw()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            // Do not update the state while a screenshot is being requested
            if (ScreenshotRequested)
                return;

            // TODO Override time so that each frame has the same duration

            base.Update(gameTime);

            if (gameTime.FrameCount == 60)
            {
                RequestScreenshot();
            }

            if (gameTime.FrameCount >= 65)
            {
                Exit();
            }
        }

        protected bool ScreenshotRequested = false;
        protected void RequestScreenshot()
        {
            ScreenshotRequested = true;
        }

        protected void SaveCurrentFrameBufferToHDD()
        {
            var Filename = xenkoDir + "\\screenshots\\" + assemblyName + "." + platformName + "_" + testName + "_" + screenShots + ".png";
            screenShots++;

            SaveTexture(GraphicsDevice.BackBuffer, Filename);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!ScreenshotRequested)
                return;

            SaveCurrentFrameBufferToHDD();
            ScreenshotRequested = false;
        }

        public static void Main()
        {
            using (var game = new GameTest("GameTest")) { game.Run(); }

            using (var game = new VisualTestInitializers()) { game.Run(); }

            using (var game = new VisualTestSpawners()) { game.Run(); }
        }
    }
}

