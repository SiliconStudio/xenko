// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;
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
        // Breaking changes
        //  Please update the version number every time there is a breaking change to the particle engine and write down what has been changed
//        const int ParticleTestVersion = 1;  // Initial tests
//        const int ParticleTestVersion = 2;  // Changed the tests on purpose to check if the tests fail
//        const int ParticleTestVersion = 3;  // Added actual visual tests, bumping up the version since they are quite different
//        const int ParticleTestVersion = 4;  // Changed the default size for billboards, hexagons and quads (previous visual tests are broken)
//        const int ParticleTestVersion = 5;  // Changed the colliders behavior (non-uniform scales weren't supported before)
//        const int ParticleTestVersion = 6;  // Moved the main update from Update() to Draw() cycle
        const int ParticleTestVersion = 7;  // Children Particles visual test updated

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
            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            //  SaveScreenshot is only defined for windows
            platformName = "Windows";
            Directory.CreateDirectory(xenkoDir + "\\screenshots\\");
#endif

            CurrentVersion = ParticleTestVersion;
            AutoLoadDefaultSettings = true;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1, };
            
            IsFixedTimeStep = true;
            ForceOneUpdatePerDraw = true;
            IsDrawDesynchronized = false;
            // This still doesn't work IsDrawDesynchronized = false; // Double negation!
            TargetElapsedTime = TimeSpan.FromTicks(10000000 / 60); // target elapsed time is by default 60Hz
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var assetManager = Services.GetSafeServiceAs<ContentManager>();

            // Make sure you have created a Scene with the same name (testName) in your XenkoGameStudio project.
            // The scene should be included in the build as Root and copied together with the other 
            //  assets to the /GameAssets directory contained in this assembly's directory
            // Finally, make sure the scene is also added to the SiliconStudio.Xenko.Particles.Tests.xkpkg
            //  and it has a proper uid. Example (for the VisualTestSpawners scene):
            //     - a9ba28ad-d83b-4957-8ed6-42863c1d903c:VisualTestSpawners
            SceneSystem.SceneInstance = new SceneInstance(Services, assetManager.Load<Scene>(testName));
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            // Take a screenshot after 60 frames
            FrameGameSystem.TakeScreenshot(60);
        }

        protected override void Update(GameTime gameTime)
        {
            // Do not update the state while a screenshot is being requested
            if (ScreenshotRequested)
                return;

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

        protected void SaveCurrentFrameBufferToHdd()
        {
            // SaveTexture is only defined for Windows and is only used to test the screenshots locally
            var filename = xenkoDir + "\\screenshots\\" + assemblyName + "." + platformName + "_" + testName + "_" + screenShots + ".png";
            screenShots++;

            SaveTexture(GraphicsDevice.Presenter.BackBuffer, filename);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!ScreenshotRequested)
                return;

            SaveCurrentFrameBufferToHdd();
            ScreenshotRequested = false;
        }

        /// <summary>
        /// This is useful if you want to run all the tests on your own machine and compare images
        /// </summary>
        public static void Main()
        {
            //using (var game = new GameTest("GameTest")) { game.Run(); }

            using (var game = new VisualTestInitializers()) { game.Run(); }

            using (var game = new VisualTestSpawners()) { game.Run(); }

            using (var game = new VisualTestGeneral()) { game.Run(); }

            using (var game = new VisualTestUpdaters()) { game.Run(); }

            using (var game = new VisualTestMaterials()) { game.Run(); }

            using (var game = new VisualTestCurves()) { game.Run(); }

            using (var game = new VisualTestRibbons()) { game.Run(); }

            using (var game = new VisualTestChildren()) { game.Run(); }
        }
    }
}

