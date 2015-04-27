using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Images;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    public class TestImageEffect : TestGameBase
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

            FrameGameSystem.Draw(DrawCustomEffect).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            hdrTexture = await Asset.LoadAsync<Texture>("Atrium");
            hdrRenderTexture = Texture.New2D(GraphicsDevice, hdrTexture.Width, hdrTexture.Height, 1, hdrTexture.Format, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            drawEffectContext = RenderContext.GetShared(Services);
            postProcessingEffects = new PostProcessingEffects(drawEffectContext);
            postProcessingEffects.BrightFilter.Threshold = 100.0f;
            postProcessingEffects.Bloom.DownScale = 2;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if(!ScreenShotAutomationEnabled)
                DrawCustomEffect();
        }

        private void DrawCustomEffect()
        {
            GraphicsDevice.Copy(hdrTexture, hdrRenderTexture);

            if (Input.IsKeyDown(Keys.Left))
            {
                postProcessingEffects.BrightFilter.Threshold -= 10.0f;
                Trace.WriteLine(string.Format("BrightFilter Threshold: {0}", postProcessingEffects.BrightFilter.Threshold));
            }
            else if (Input.IsKeyDown(Keys.Right))
            {
                postProcessingEffects.BrightFilter.Threshold += 10.0f;
                Trace.WriteLine(string.Format("BrightFilter Threshold: {0}", postProcessingEffects.BrightFilter.Threshold));
            }

            postProcessingEffects.Bloom.Enabled = !Input.IsKeyDown(Keys.Space);
            postProcessingEffects.Bloom.ShowOnlyBloom = !Input.IsKeyDown(Keys.B);
            if (Input.IsKeyDown(Keys.Down))
            {
                postProcessingEffects.Bloom.Amount += -0.01f;
                Trace.WriteLine(string.Format("Bloom Amount: {0}", postProcessingEffects.Bloom.Amount));
            }
            else if (Input.IsKeyDown(Keys.Up))
            {
                postProcessingEffects.Bloom.Amount += +0.01f;
                Trace.WriteLine(string.Format("Bloom Amount: {0}", postProcessingEffects.Bloom.Amount));
            }


            postProcessingEffects.SetInput(hdrRenderTexture);
            postProcessingEffects.SetOutput(GraphicsDevice.BackBuffer);
            postProcessingEffects.Draw();
        }

        public static void Main()
        {
            using (var game = new TestImageEffect())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test, Ignore]
        public void RunImageEffect()
        {
            RunGameTest(new TestImageEffect());
        }
    }
}