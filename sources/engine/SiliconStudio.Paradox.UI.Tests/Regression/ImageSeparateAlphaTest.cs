// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="UIImage"/> with separate alpha texture
    /// </summary>
    [TestFixture]
    public class ImageSeparateAlphaTest : UnitTestGameBase
    {
        public ImageSeparateAlphaTest()
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

            var uiImages = Asset.Load<UIImageGroup>("UIImages");

            var imgElt1 = new ImageElement { Source = uiImages["GameScreen"], StretchType = StretchType.Fill };
            imgElt1.DependencyProperties.Set(Panel.ZIndexPropertyKey, 0);
            imgElt1.DependencyProperties.Set(GridBase.RowSpanPropertyKey, 3);
            imgElt1.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 3);
            imgElt1.DependencyProperties.Set(GridBase.RowPropertyKey, 0);
            imgElt1.DependencyProperties.Set(GridBase.ColumnPropertyKey, 0);

            var imgElt2 = new ImageElement { Source = uiImages["Logo"], StretchType = StretchType.Fill };
            imgElt2.DependencyProperties.Set(Panel.ZIndexPropertyKey, 1);
            imgElt2.DependencyProperties.Set(GridBase.RowPropertyKey, 0);
            imgElt2.DependencyProperties.Set(GridBase.ColumnPropertyKey, 2);

            var imgElt3 = new ImageElement { Source = uiImages["Logo"], StretchType = StretchType.Fill };
            imgElt3.DependencyProperties.Set(Panel.ZIndexPropertyKey, 2);
            imgElt3.DependencyProperties.Set(GridBase.RowPropertyKey, 2);
            imgElt3.DependencyProperties.Set(GridBase.ColumnPropertyKey, 0);

            var imgElt4 = new ImageElement { Source = uiImages["BorderButton"], StretchType = StretchType.Fill };
            imgElt4.DependencyProperties.Set(Panel.ZIndexPropertyKey, 3);
            imgElt4.DependencyProperties.Set(GridBase.RowPropertyKey, 1);
            imgElt4.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);

            var grid = new UniformGrid { Rows = 3, Columns = 3 };
            grid.Children.Add(imgElt1);
            grid.Children.Add(imgElt2);
            grid.Children.Add(imgElt3);
            grid.Children.Add(imgElt4);

            UIComponent.RootElement = grid;
        }

        [Test]
        public void RunSeparateAlphaUnitTest()
        {
            RunGameTest(new ImageSeparateAlphaTest());
        }

        public static void Main()
        {
            var separateAlphaUnitTest = new ImageSeparateAlphaTest();
            separateAlphaUnitTest.Run();
        }
    }
}