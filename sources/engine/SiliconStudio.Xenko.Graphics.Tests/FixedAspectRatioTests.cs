using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [TestFixture]
    public class FixedAspectRatioTests : GameTestBase
    {
        protected Scene Scene;

        public FixedAspectRatioTests()
        {
            CurrentVersion = 1;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Force aspect ratio
            SceneSystem.GraphicsCompositor = GraphicsCompositorHelper.CreateDefault(false, clearColor: Color.Green, graphicsProfile: GraphicsProfile.Level_9_1);
            SceneSystem.GraphicsCompositor.Game = new ForceAspectRatioSceneRenderer { Child = SceneSystem.GraphicsCompositor.Game, FixedAspectRatio = 3.0f, ForceAspectRatio = true };

            Scene = new Scene();

            Texture png;
            using (var pngStream = ContentManager.FileProvider.OpenStream("PngImage", VirtualFileMode.Open, VirtualFileAccess.Read))
            using (var pngImage = Image.Load(pngStream, GraphicsDevice.ColorSpace == ColorSpace.Linear))
                png = Texture.New(GraphicsDevice, pngImage);

            var camera = new Entity { new CameraComponent { Slot = SceneSystem.GraphicsCompositor.Cameras[0].ToSlotId() } };
            var plane = new Entity { new BackgroundComponent { Texture = png } };
            Scene.Entities.Add(plane);
            Scene.Entities.Add(camera);

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);
        }

        [Test]
        public void TestFixedRatio()
        {
            RunGameTest(new FixedAspectRatioTests());
        }

        public static void Main(string[] args)
        {
            using (Game game = new FixedAspectRatioTests())
            {
                game.Run();
            }
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            // Take screenshot first frame
            FrameGameSystem.TakeScreenshot();
        }
    }
}
