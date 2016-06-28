// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ImageButton"/> 
    /// </summary>
    public class ImageButtonTest : UITestGameBase
    {
        private ImageButton imageButton;

        public ImageButtonTest()
        {
            CurrentVersion = 3;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            imageButton = new ImageButton
            {
                PressedImage = (SpriteFromTexture)new Sprite(Content.Load<Texture>("ImageButtonPressed")),
                NotPressedImage = (SpriteFromTexture)new Sprite(Content.Load<Texture>("ImageButtonNotPressed")),
            };

            UIComponent.RootElement = imageButton;
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
