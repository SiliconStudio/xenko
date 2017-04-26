// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.NextGen;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public class TestLightShafts : GraphicTestGameBase
    {
        public TestLightShafts()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            //GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();

            SceneSystem.InitialSceneUrl = "LightShafts";
            SceneSystem.GraphicsCompositor = GraphicsCompositorHelper.CreateDefault(true);
            var fwr = ((SceneSystem.GraphicsCompositor.Game as SceneCameraRenderer).Child as ForwardRenderer);
            fwr.LightShafts = new LightShafts();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            ProfilerSystem.EnableProfiling(false, GameProfilingKeys.GameDrawFPS);

            Window.AllowUserResizing = true;

            var cameraEntity = SceneSystem.SceneInstance.First(x => x.Get<CameraComponent>() != null);
            cameraEntity.Add(new FpsTestCamera());
            //cameraEntity.Transform.Position = new Vector3(0.0f, 5.0f, 10.0f);
            //cameraEntity.Transform.Rotation = Quaternion.Identity;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public static void Main()
        {
            using (var game = new TestLightShafts())
                game.Run();
        }
    }
}
