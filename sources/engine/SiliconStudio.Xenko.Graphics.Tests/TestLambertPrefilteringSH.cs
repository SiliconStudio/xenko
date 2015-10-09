// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Paradox.Rendering.Images.SphericalHarmonics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    public class TestLambertPrefilteringSH : TestGameBase
    {
        private SpriteBatch spriteBatch;

        private RenderContext drawEffectContext;

        private Texture inputCubemap;

        private Texture displayedCubemap;

        private LambertianPrefilteringSH lamberFilter;
        private SphericalHarmonicsRendererEffect renderSHEffect;

        private Texture outputCubemap;

        private bool shouldPrefilter = true;

        private Int2 screenSize = new Int2(768, 1024);

        private Effect cubemapSpriteEffect;

        public TestLambertPrefilteringSH()
        {
            CurrentVersion = 2;
            GraphicsDeviceManager.PreferredBackBufferWidth = screenSize.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenSize.Y;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            cubemapSpriteEffect = EffectSystem.LoadEffect("CubemapSprite").WaitForResult();

            drawEffectContext = RenderContext.GetShared(Services);
            lamberFilter = new LambertianPrefilteringSH(drawEffectContext);
            renderSHEffect = new SphericalHarmonicsRendererEffect();
            renderSHEffect.Initialize(drawEffectContext);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            inputCubemap = Asset.Load<Texture>("CubeMap");
            outputCubemap = Texture.NewCube(GraphicsDevice, 256, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource).DisposeBy(this);
            displayedCubemap = outputCubemap;
        }

        private void PrefilterCubeMap()
        {
            if (!shouldPrefilter)
                return;

            lamberFilter.HarmonicOrder = 5;
            lamberFilter.RadianceMap = inputCubemap;
            lamberFilter.Draw();

            renderSHEffect.InputSH = lamberFilter.PrefilteredLambertianSH;
            renderSHEffect.SetOutput(outputCubemap);
            renderSHEffect.Draw();

            //shouldPrefilter = false;
            //displayedCubemap = outputCubemap;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        private void RenderCubeMap()
        {
            if (displayedCubemap == null || spriteBatch == null)
                return;

            var size = new Vector2(screenSize.X / 3f, screenSize.Y / 4f);
            
            GraphicsDevice.Parameters.Set(CubemapSpriteKeys.ViewIndex, 1);
            spriteBatch.Begin(SpriteSortMode.Texture, cubemapSpriteEffect);
            spriteBatch.Draw(displayedCubemap, new RectangleF(0, size.Y, size.X, size.Y), Color.White);
            spriteBatch.End();

            GraphicsDevice.Parameters.Set(CubemapSpriteKeys.ViewIndex, 2);
            spriteBatch.Begin(SpriteSortMode.Texture, cubemapSpriteEffect);
            spriteBatch.Draw(displayedCubemap, new RectangleF(size.X, 0f, size.X, size.Y), Color.White);
            spriteBatch.End();

            GraphicsDevice.Parameters.Set(CubemapSpriteKeys.ViewIndex, 4);
            spriteBatch.Begin(SpriteSortMode.Texture, cubemapSpriteEffect);
            spriteBatch.Draw(displayedCubemap, new RectangleF(size.X, size.Y, size.X, size.Y), Color.White);
            spriteBatch.End();

            GraphicsDevice.Parameters.Set(CubemapSpriteKeys.ViewIndex, 3);
            spriteBatch.Begin(SpriteSortMode.Texture, cubemapSpriteEffect);
            spriteBatch.Draw(displayedCubemap, new RectangleF(size.X, 2f * size.Y, size.X, size.Y), Color.White);
            spriteBatch.End();

            GraphicsDevice.Parameters.Set(CubemapSpriteKeys.ViewIndex, 5);
            spriteBatch.Begin(SpriteSortMode.Texture, cubemapSpriteEffect);
            spriteBatch.Draw(displayedCubemap, new RectangleF(size.X, 3f * size.Y, size.X, size.Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
            spriteBatch.End();

            GraphicsDevice.Parameters.Set(CubemapSpriteKeys.ViewIndex, 0);
            spriteBatch.Begin(SpriteSortMode.Texture, cubemapSpriteEffect);
            spriteBatch.Draw(displayedCubemap, new RectangleF(2f * size.X, size.Y, size.X, size.Y), Color.White);
            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.Space))
                shouldPrefilter = true;

            if (Input.IsKeyPressed(Keys.I))
                displayedCubemap = inputCubemap;

            if (Input.IsKeyPressed(Keys.O))
                displayedCubemap = outputCubemap;

            if(Input.IsKeyPressed(Keys.S))
                SaveTexture(GraphicsDevice.BackBuffer, "LambertianPrefilteredImageCross.png");
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            PrefilterCubeMap();
            RenderCubeMap();
        }

        [Test]
        public void RunTestPass2()
        {
            RunGameTest(new TestLambertPrefilteringSH());
        }

        public static void Main()
        {
            using (var game = new TestLambertPrefilteringSH())
                game.Run();
        }
    }
}