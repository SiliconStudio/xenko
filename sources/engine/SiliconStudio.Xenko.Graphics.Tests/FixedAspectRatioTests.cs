using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class FixedAspectRatioTests : GameTestBase
    {
        private SceneGraphicsCompositorLayers graphicsCompositor;
        protected Scene Scene;

        public FixedAspectRatioTests()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            graphicsCompositor = new SceneGraphicsCompositorLayers
            {
                Master =
                {
                    Renderers =
                    {
                        new ClearRenderFrameRenderer { Color = Color.Green, Name = "Clear frame" },
                        new SceneCameraRenderer { Mode = new CameraRendererModeForward { Name = "Camera renderer" }, FixedAspectRatio = 3.0f, ForceAspectRatio = true }
                    }
                }
            };

            Scene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };

            Texture png;
            using (var pngStream = ContentManager.FileProvider.OpenStream("PngImage", VirtualFileMode.Open, VirtualFileAccess.Read))
            using (var pngImage = Image.Load(pngStream, GraphicsDevice.ColorSpace == ColorSpace.Linear))
                png = Texture.New(GraphicsDevice, pngImage);

            var plane = new Entity { new BackgroundComponent { Texture = png } };
            Scene.Entities.Add(plane);

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);
        }

        [Test]
        public void TestFixedRatio()
        {
            RunGameTest(new FixedAspectRatioTests());
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            // Take screenshot first frame
            FrameGameSystem.TakeScreenshot();
        }
    }
}
