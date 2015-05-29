// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ScrollViewer"/> 
    /// </summary>
    public class ImageRotatedTest : UnitTestGameBase
    {
        public ImageRotatedTest()
        {
            CurrentVersion = 5;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var uiImages = Asset.Load<UIImageGroup>("RotatedImages");
            var img1 = new ImageElement { Source = uiImages["NotRotated"], StretchType = StretchType.Fill };
            var img2 = new ImageElement { Source = uiImages["Rotated90"], StretchType = StretchType.Fill };

            img2.DependencyProperties.Set(GridBase.RowPropertyKey, 1);

            var grid = new UniformGrid { Rows = 2 };
            grid.Children.Add(img1);
            grid.Children.Add(img2);

            UIComponent.RootElement = grid;
        }

        [Test]
        public void RunImageRotatedTest()
        {
            RunGameTest(new ImageRotatedTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ImageRotatedTest())
                game.Run();
        }
    }
}