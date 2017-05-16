// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;

using NUnit.Framework;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="Button"/> 
    /// </summary>
    public class UniformGridTest : UITestGameBase
    {
        public UniformGridTest()
        {
            //CurrentVersion = 8;
            CurrentVersion = 9; // One texture was using Format: TrueColor which is no longer available!
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var imgElt = new ImageElement { Source = (SpriteFromTexture)new Sprite(Content.Load<Texture>("uv")), StretchType = StretchType.Fill };
            imgElt.DependencyProperties.Set(GridBase.RowSpanPropertyKey, 2);
            imgElt.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 2);
            imgElt.DependencyProperties.Set(GridBase.RowPropertyKey, 1);
            imgElt.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);

            var button1 = new Button();
            ApplyButtonDefaultStyle(button1);
            button1.DependencyProperties.Set(GridBase.RowPropertyKey, 3);
            button1.DependencyProperties.Set(GridBase.ColumnPropertyKey, 0);

            var button2 = new Button();
            ApplyButtonDefaultStyle(button2);
            button2.DependencyProperties.Set(GridBase.RowPropertyKey, 3);
            button2.DependencyProperties.Set(GridBase.ColumnPropertyKey, 3);

            var text = new TextBlock
            {
                Text = "Test Uniform Grid", 
                Font = Content.Load<SpriteFont>("MicrosoftSansSerif15"), 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            ApplyTextBlockDefaultStyle(text);
            text.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 2);
            text.DependencyProperties.Set(GridBase.RowPropertyKey, 0);
            text.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);

            var grid = new UniformGrid { Rows = 4, Columns = 4};
            grid.Children.Add(imgElt);
            grid.Children.Add(button1);
            grid.Children.Add(button2);
            grid.Children.Add(text);

            UIComponent.Page = new Engine.UIPage { RootElement = grid };
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
