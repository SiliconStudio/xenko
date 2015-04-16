// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ImageElement"/> 
    /// </summary>
    public class ImageTest : UnitTestGameBase
    {
        public ImageTest()
        {
            CurrentVersion = 2;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            UIComponent.RootElement = new ImageElement { Source = new UIImage(Asset.Load<Texture>("uv"))};
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        [Test]
        public void RunImageTest()
        {
            RunGameTest(new ImageTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ImageTest())
                game.Run();
        }
    }
}
