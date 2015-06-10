// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    class TestTextureAlphaSeparation : EngineTestBase
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

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            var dice = Asset.Load<Entity>("Cube/cube");

            var cameraComp = Camera.Get<CameraComponent>();
            cameraComp.UseCustomAspectRatio = true;
            cameraComp.AspectRatio = 1;
            cameraComp.FarClipPlane = 100;
            cameraComp.NearClipPlane = 0.1f;
            cameraComp.UseCustomViewMatrix = true;
            cameraComp.VerticalFieldOfView = MathUtil.GradiansToDegrees(1);
            cameraComp.ViewMatrix = Matrix.LookAtRH(new Vector3(0, 0, -2), dice.Transform.Position, -Vector3.UnitY);

            Scene.AddChild(dice);
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