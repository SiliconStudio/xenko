// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Regression
{
    [TestFixture]
    public abstract class GraphicsTestBase : TestGameBase
    {
        #region Public properties

        public static bool ForceInteractiveMode;

        public FrameGameSystem FrameGameSystem { get; private set; }

        protected TestContext CurrentTestContext { get; set; }
        
        #endregion

        #region Public members

        /// <summary>
        /// The current version of the test
        /// </summary>
        public int CurrentVersion;

        /// <summary>
        /// The current version extra parameter (concatenated to CurrentVersionNumber).
        /// </summary>
        public string CurrentVersionExtra;

        public int FrameIndex;

        private bool screenshotAutomationEnabled;

        #endregion

        #region Constructors

        protected GraphicsTestBase()
        {
            CurrentVersion = 0;

            FrameGameSystem = new FrameGameSystem(Services);
            GameSystems.Add(FrameGameSystem);

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            // get build number
            int buildNumber;
            if (ImageTester.ImageTestResultConnection.BuildNumber <= 0 && Int32.TryParse(Environment.GetEnvironmentVariable("PARADOX_BUILD_NUMBER"), out buildNumber))
                ImageTester.ImageTestResultConnection.BuildNumber = buildNumber;

            // get branch name
            if (String.IsNullOrEmpty(ImageTester.ImageTestResultConnection.BranchName))
                ImageTester.ImageTestResultConnection.BranchName = Environment.GetEnvironmentVariable("PARADOX_BRANCH_NAME") ?? "";
#endif
        }

        #endregion

        #region public methods

        /// <summary>
        /// Save the image locally or on the server.
        /// </summary>
        /// <param name="textureToSave">The texture to save.</param>
        public void SaveImage(Texture textureToSave)
        {
            if (textureToSave == null)
                return;

            TestGameLogger.Info(@"Saving non null image");
            var testName = CurrentTestContext != null ? CurrentTestContext.Test.FullName : null;
            TestGameLogger.Info(@"saving remotely.");
            using (var image = textureToSave.GetDataAsImage())
            {
                try
                {
                    SendImage(image, testName);
                }
                catch (Exception)
                {
                    TestGameLogger.Error(@"An error occurred when trying to send the data to the server.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Save the image locally or on the server.
        /// </summary>
        public void SaveBackBuffer()
        {
            TestGameLogger.Info(@"Saving the backbuffer");
            SaveImage(GraphicsDevice.BackBuffer);
        }

        /// <summary>
        /// Gets or sets the value indicating if the screen shots automation should be enabled or not.
        /// </summary>
        public bool ScreenShotAutomationEnabled
        {
            get
            {
                return screenshotAutomationEnabled;
            }
            set
            {
                FrameGameSystem.Visible = value;
                FrameGameSystem.Enabled = value;
                screenshotAutomationEnabled = value;
            }
        }

        #endregion

        #region Protected methods
        
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            Window.Position = Int2.Zero; // avoid possible side effects due to position of the window in the screen.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            // Register 3D card name
            ImageTester.ImageTestResultConnection.DeviceName += "_" + GraphicsDevice.Adapter.Description;
#endif

            Script.Add(RegisterTestsInternal);
        }

        private Task RegisterTestsInternal()
        {
            RegisterTests();

            return Task.FromResult(true);
        }

        /// <summary>
        /// Loop through all the tests and save the images.
        /// </summary>
        /// <param name="gameTime">the game time.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!ScreenShotAutomationEnabled)
                return;

            if (FrameGameSystem.AllTestsCompleted)
                Exit();
            else if (FrameGameSystem.TakeSnapshot)
                SaveBackBuffer();
        }

        /// <summary>
        /// Method to register the tests.
        /// </summary>
        protected virtual void RegisterTests()
        {
        }

        protected static void RunGameTest(GraphicsTestBase game)
        {
            game.CurrentTestContext = TestContext.CurrentContext;

            game.ScreenShotAutomationEnabled = !ForceInteractiveMode;

            GameTester.RunGameTest(game);

            if (game.ScreenShotAutomationEnabled)
                Assert.IsTrue(ImageTester.RequestImageComparisonStatus(game.CurrentTestContext.Test.FullName), "The image comparison returned false.");
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Send the data of the test to the server.
        /// </summary>
        /// <param name="image">The image to send.</param>
        /// <param name="testName">The name of the test.</param>
        public void SendImage(Image image, string testName)
        {
            var currentVersion = CurrentVersion.ToString();
            if (CurrentVersionExtra != null)
                currentVersion += CurrentVersionExtra;

            // TODO: Allow textual frame names (and use FrameIndex if not properly set)
            var frameIndex = FrameIndex++;

            ImageTester.SendImage(new TestResultImage { CurrentVersion = currentVersion, Frame = frameIndex.ToString(), Image = image, TestName = testName });
        }

        #endregion

        #region Helper structures and classes

        /// <summary>
        /// A structure to store information about the connected test devices.
        /// </summary>
        public struct ConnectedDevice
        {
            public string Serial;
            public string Name;
            public TestPlatform Platform;

            public override string ToString()
            {
                return Name + " " + Serial + " " + PlatformPermutator.GetPlatformName(Platform);
            }
        }

        #endregion
    }
}
