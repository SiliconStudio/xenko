// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    class TestTextureAlphaSeparation : GraphicsTestBase
    {
        public TestTextureAlphaSeparation()
        {
            CurrentVersion = 2;

            GraphicsDeviceManager.PreferredBackBufferWidth = 800;
            GraphicsDeviceManager.PreferredBackBufferHeight = 800;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.None;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
            GraphicsDeviceManager.ShaderProfile = GraphicsProfile.Level_9_1;
        }

        protected void LoadDices()
        {
            RenderPipelineFactory.CreateSimple(this, "Default", Color.Red);

            var dice = Asset.Load<Entity>("Cube/cube");

            var cameraComp = new CameraComponent { AspectRatio = 1, FarPlane = 100, NearPlane = 0.1f, TargetUp = -Vector3.UnitY, Target = dice, UseViewMatrix = false, VerticalFieldOfView = 1 };
            RenderSystem.Pipeline.SetCamera(cameraComp);

            var cameraEntity = new Entity { Transform = { Translation = new Vector3(0, 0, -2) } };
            cameraEntity.Add(cameraComp);

            Entities.Add(dice);
            Entities.Add(cameraEntity);
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            LoadDices();
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new TestTextureAlphaSeparation())
                game.Run();
        }

        /// <summary>
        /// Run the regression test
        /// </summary>
        [Test]
        public void RunTextureAlphaSeparationTest()
        {
            RunGameTest(new TestTextureAlphaSeparation());
        }
    }
}