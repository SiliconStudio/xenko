// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ImageButton"/> 
    /// </summary>
    public class ImageButtonTest : UnitTestGameBase
    {
        private ImageButton imageButton;

        public ImageButtonTest()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            imageButton = new ImageButton
            {
                PressedImage = new UIImage(Asset.Load<Texture>("ImageButtonPressed")),
                NotPressedImage = new UIImage(Asset.Load<Texture>("ImageButtonNotPressed")),
            };

            UI.RootElement = imageButton;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.TakeScreenshot();
            FrameGameSystem.Draw(DrawTest1).TakeScreenshot();
        }

        public void DrawTest1()
        {
            imageButton.RaiseTouchDownEvent(new TouchEventArgs());
        }

        [Test]
        public void RunImageButtonTest()
        {
            RunGameTest(new ImageButtonTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ImageButtonTest())
                game.Run();
        }
    }
}