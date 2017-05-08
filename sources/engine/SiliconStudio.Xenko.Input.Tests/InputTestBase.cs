// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Input.Tests
{
    public class InputTestBase : GameTestBase
    {
        private const float TextSpaceY = 3;
        private const float TextSubSectionOffsetX = 15;

        protected Vector2 TextLeftTopCorner = new Vector2(5, 5);
        protected Color MouseColor = Color.Gray;
        protected Color DefaultTextColor = Color.Black;
        protected int LineOffset;

        private float textHeight;

        protected InputTestBase()
        {
        }

        protected SpriteBatch SpriteBatch { get; private set; }
        protected SpriteFont SpriteFont { get; private set; }
        protected Vector2 ScreenSize { get; private set; }
        protected Texture RoundTexture { get; private set; }
        protected Vector2 RoundTextureSize { get; private set; }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Load the fonts
            SpriteFont = Content.Load<SpriteFont>("Arial");

            // load the round texture 
            RoundTexture = Content.Load<Texture>("round");

            // create the SpriteBatch used to render them
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // initialize parameters
            textHeight = SpriteFont.MeasureString("Measure Text Height (dummy string)").Y;
            ScreenSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
            RoundTextureSize = new Vector2(RoundTexture.Width, RoundTexture.Height);
        }

        protected void BeginSpriteBatch()
        {
            SpriteBatch.Begin(GraphicsContext);
            LineOffset = 0;
        }

        protected void EndSpriteBatch()
        {
            SpriteBatch.End();
        }

        protected void DrawCursor()
        {
            var mousePosition = Input.MousePosition;
            var mouseScreenPosition = new Vector2(mousePosition.X * ScreenSize.X, mousePosition.Y * ScreenSize.Y);
            SpriteBatch.Draw(RoundTexture, mouseScreenPosition, MouseColor, 0, RoundTextureSize / 2, 0.1f);
        }

        protected void WriteLine(string str, int indent = 0)
        {
            WriteLine(str, DefaultTextColor, indent);
        }

        protected void WriteLine(int line, string str, int indent = 0)
        {
            WriteLine(line, str, DefaultTextColor, indent);
        }

        protected void WriteLine(string str, Color color, int indent = 0)
        {
            WriteLine(LineOffset++, str, color, indent);
        }

        protected void WriteLine(int line, string str, Color color, int indent = 0)
        {
            SpriteBatch.DrawString(SpriteFont, str, TextLeftTopCorner + new Vector2(TextSubSectionOffsetX * indent, line * (textHeight + TextSpaceY)), color);
        }
    }
}
