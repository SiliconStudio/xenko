// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="Button"/> 
    /// </summary>
    public class UniformGridTest : UnitTestGameBase
    {
        public UniformGridTest()
        {
            CurrentVersion = 4;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var imgElt = new ImageElement { Source = new UIImage(Asset.Load<Texture>("uv")), StretchType = StretchType.Fill };
            imgElt.DependencyProperties.Set(GridBase.RowSpanPropertyKey, 2);
            imgElt.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 2);
            imgElt.DependencyProperties.Set(GridBase.RowPropertyKey, 1);
            imgElt.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);

            var button1 = new Button();
            button1.DependencyProperties.Set(GridBase.RowPropertyKey, 3);
            button1.DependencyProperties.Set(GridBase.ColumnPropertyKey, 0);

            var button2 = new Button();
            button2.DependencyProperties.Set(GridBase.RowPropertyKey, 3);
            button2.DependencyProperties.Set(GridBase.ColumnPropertyKey, 3);

            var text = new TextBlock
            {
                Text = "Test Uniform Grid", 
                Font = Asset.Load<SpriteFont>("MicrosoftSansSerif15"), 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            text.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 2);
            text.DependencyProperties.Set(GridBase.RowPropertyKey, 0);
            text.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);

            var grid = new UniformGrid { Rows = 4, Columns = 4};
            grid.Children.Add(imgElt);
            grid.Children.Add(button1);
            grid.Children.Add(button2);
            grid.Children.Add(text);

            UIComponent.RootElement = grid;
        }

        [Test]
        public void RunUniformGridTest()
        {
            RunGameTest(new UniformGridTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new UniformGridTest())
                game.Run();
        }
    }
}