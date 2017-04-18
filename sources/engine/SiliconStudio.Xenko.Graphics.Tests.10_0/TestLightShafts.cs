// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
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
            CurrentVersion = 1;

            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_10_0 };
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();

            SceneSystem.InitialGraphicsCompositorUrl = "LightShaftsGraphicsCompositor";
            SceneSystem.InitialSceneUrl = "LightShafts";
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            Window.AllowUserResizing = true;

            var cameraEntity = SceneSystem.SceneInstance.First(x => x.Get<CameraComponent>() != null);
            cameraEntity.Add(new FpsTestCamera() {MoveSpeed = 10.0f });
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.TakeScreenshot(2);
        }

        public static void Main()
        {
            using (var game = new TestLightShafts())
                game.Run();
        }
        
        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunLightShafts()
        {
            RunGameTest(new TestLightShafts());
        }
    }
}