using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [TestFixture]
    public class TestImageEffect : TestGameBase
    {
        private ImageEffectContext imageEffectContext;

        private Texture hdrTexture;

        private Texture hdrRenderTexture;
        private ImageEffectBundle imageEffectBundle;

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

            hdrTexture = Texture.Load(GraphicsDevice, File.OpenRead(@"C:\Code\Paradox\sources\engine\SiliconStudio.Paradox.Graphics.Tests\Assets\AtriumNight.dds")); //await Asset.LoadAsync<Texture>("Atrium");
            hdrRenderTexture = Texture.New2D(GraphicsDevice, hdrTexture.Width, hdrTexture.Height, 1, hdrTexture.Format, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            imageEffectContext = new ImageEffectContext(this);
            imageEffectBundle = new ImageEffectBundle(imageEffectContext);
            imageEffectBundle.BrightFilter.Threshold = 100.0f;
            imageEffectBundle.Bloom.DownScale = 2;
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
                imageEffectBundle.BrightFilter.Threshold -= 10.0f;
                Trace.WriteLine(string.Format("BrightFilter Threshold: {0}", imageEffectBundle.BrightFilter.Threshold));
            }
            else if (Input.IsKeyDown(Keys.Right))
            {
                imageEffectBundle.BrightFilter.Threshold += 10.0f;
                Trace.WriteLine(string.Format("BrightFilter Threshold: {0}", imageEffectBundle.BrightFilter.Threshold));
            }

            imageEffectBundle.Bloom.Enabled = !Input.IsKeyDown(Keys.Space);
            imageEffectBundle.Bloom.ShowOnlyBloom = !Input.IsKeyDown(Keys.B);
            if (Input.IsKeyDown(Keys.Down))
            {
                imageEffectBundle.Bloom.Amount += -0.01f;
                Trace.WriteLine(string.Format("Bloom Amount: {0}", imageEffectBundle.Bloom.Amount));
            }
            else if (Input.IsKeyDown(Keys.Up))
            {
                imageEffectBundle.Bloom.Amount += +0.01f;
                Trace.WriteLine(string.Format("Bloom Amount: {0}", imageEffectBundle.Bloom.Amount));
            }


            imageEffectBundle.SetInput(hdrRenderTexture);
            imageEffectBundle.SetOutput(GraphicsDevice.BackBuffer);
            imageEffectBundle.Draw();
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