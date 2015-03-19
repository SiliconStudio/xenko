// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    public class TestSprite : TestGameBase
    {
        private SpriteGroup spriteUv;
        private SpriteGroup spriteSphere;

        private SpriteBatch spriteBatch;

        public TestSprite()
        {
            CurrentVersion = 3;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Draw(DrawSprites).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            spriteUv = Asset.Load<SpriteGroup>("SpriteUV");
            spriteSphere = Asset.Load<SpriteGroup>("SpriteSphere");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawSprites();
        }

        private void DrawSprites()
        {
            const int spaceSpan = 5;

            GraphicsDevice.Clear(GraphicsDevice.BackBuffer, Color.Black);
            GraphicsDevice.Clear(GraphicsDevice.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, GraphicsDevice.BackBuffer);

            spriteBatch.Begin(SpriteSortMode.Texture, GraphicsDevice.BlendStates.AlphaBlend);

            var spriteUvSize = new Vector2(spriteUv.Images[0].Region.Width, spriteUv.Images[0].Region.Height);
            var spriteSphereSize = new Vector2(spriteSphere.Images[0].Region.Width, spriteSphere.Images[0].Region.Height);

            // draw sprite using frame index
            var positionUv = new Vector2(spaceSpan + spriteUvSize.X/2, spaceSpan + spriteUvSize.Y/2);
            spriteUv.Images[0].Draw(spriteBatch, positionUv);

            positionUv.X += spriteUvSize.X + spaceSpan;
            spriteUv.Images[1].Draw(spriteBatch, positionUv);

            positionUv.X += spriteUvSize.X + spaceSpan;
            spriteUv.Images[2].Draw(spriteBatch, positionUv);

            positionUv.X += spriteUvSize.X + spaceSpan;
            spriteUv.Images[3].Draw(spriteBatch, positionUv);

            // draw spheres
            positionUv.X = spaceSpan + spriteUvSize.X/2;
            positionUv.Y += spriteUvSize.Y + spaceSpan;
            spriteUv.Images[0].Draw(spriteBatch, positionUv, depthLayer: -2);

            var positionSphere = positionUv + new Vector2(spriteUvSize.X / 2, 0);
            spriteSphere.Images[0].Draw(spriteBatch, positionSphere, depthLayer: -1);

            positionUv.X += spaceSpan + spriteUvSize.X;
            spriteUv.Images[0].Draw(spriteBatch, positionUv, spriteEffects: SpriteEffects.FlipVertically);

            positionSphere = positionUv + new Vector2(spriteSphereSize.X + spaceSpan, 0);
            spriteSphere.Images[0].Draw(spriteBatch, positionSphere, (float)Math.PI / 2);

            positionSphere.X += spriteSphereSize.X + spaceSpan;
            spriteSphere.Images[0].Draw(spriteBatch, positionSphere, Color.GreenYellow, Vector2.One);

            positionSphere.X += spriteSphereSize.X + spaceSpan;
            spriteSphere.Images[0].Draw(spriteBatch, positionSphere, Color.White, new Vector2(0.66f, 0.33f), depthLayer: 1);
            
            positionSphere.X = spaceSpan;
            positionSphere.Y += 1.5f * spriteSphereSize.Y;
            spriteSphere.Images[0].Center = new Vector2(0, spriteSphereSize.Y);
            spriteSphere.Images[0].Draw(spriteBatch, positionSphere, depthLayer: 1);
            spriteSphere.Images[0].Center = new Vector2(spriteSphereSize.X / 2, spriteSphereSize.Y / 2);

            spriteBatch.End();
        }

        public static void Main()
        {
            using (var game = new TestSprite())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestSprite()
        {
            RunGameTest(new TestSprite());
        }
    }
}