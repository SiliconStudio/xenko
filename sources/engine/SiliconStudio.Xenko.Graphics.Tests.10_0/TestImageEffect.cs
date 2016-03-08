// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class TestImageEffect : GraphicTestGameBase
    {
        private RenderContext drawEffectContext;

        private Texture hdrTexture;
        private Texture hdrRenderTexture;

        private PostProcessingEffects postProcessingEffects;

        public TestImageEffect()
        {
            CurrentVersion = 1;
            GraphicsDeviceManager.PreferredBackBufferWidth = 760;
            GraphicsDeviceManager.PreferredBackBufferHeight = 1016;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            hdrTexture = Content.Load<Texture>("HdrTexture");
            hdrRenderTexture = Texture.New2D(GraphicsDevice, hdrTexture.Width, hdrTexture.Height, 1, hdrTexture.Description.Format, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            drawEffectContext = RenderContext.GetShared(Services);
            postProcessingEffects = new PostProcessingEffects(drawEffectContext);
            postProcessingEffects.BrightFilter.Threshold = 100.0f;
            postProcessingEffects.Bloom.DownScale = 2;
            postProcessingEffects.Bloom.Enabled = true;
            postProcessingEffects.Bloom.ShowOnlyBloom = true;
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!ScreenShotAutomationEnabled)
                AdjustEffectParameters();

            var renderDrawContext = new RenderDrawContext(Services, RenderContext.GetShared(Services), GraphicsContext);
            DrawCustomEffect(renderDrawContext);

            base.Draw(gameTime);
        }

        private void AdjustEffectParameters()
        {
            if (Input.IsKeyDown(Keys.Left))
            {
                postProcessingEffects.BrightFilter.Threshold -= 10.0f;
                Log.Info("BrightFilter Threshold: {0}", postProcessingEffects.BrightFilter.Threshold);
            }
            else if (Input.IsKeyDown(Keys.Right))
            {
                postProcessingEffects.BrightFilter.Threshold += 10.0f;
                Log.Info("BrightFilter Threshold: {0}", postProcessingEffects.BrightFilter.Threshold);
            }

            postProcessingEffects.Bloom.Enabled = !Input.IsKeyDown(Keys.Space);
            postProcessingEffects.Bloom.ShowOnlyBloom = !Input.IsKeyDown(Keys.B);
            if (Input.IsKeyDown(Keys.Down))
            {
                postProcessingEffects.Bloom.Amount += -0.01f;
                Log.Info("Bloom Amount: {0}", postProcessingEffects.Bloom.Amount);
            }
            else if (Input.IsKeyDown(Keys.Up))
            {
                postProcessingEffects.Bloom.Amount += +0.01f;
                Log.Info("Bloom Amount: {0}", postProcessingEffects.Bloom.Amount);
            }
        }
        private void DrawCustomEffect(RenderDrawContext context)
        {
            GraphicsContext.CommandList.CopyRegion(hdrTexture, 0, null, hdrRenderTexture, 0);

            postProcessingEffects.SetInput(hdrRenderTexture);
            postProcessingEffects.SetOutput(GraphicsContext.CommandList.RenderTarget);
            postProcessingEffects.Draw(context);
        }

        public static void Main()
        {
            using (var game = new TestImageEffect())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunImageEffect()
        {
            RunGameTest(new TestImageEffect());
        }
    }
}