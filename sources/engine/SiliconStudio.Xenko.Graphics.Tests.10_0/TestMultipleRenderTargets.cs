// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public class TestMultipleRenderTargets : GraphicTestGameBase
    {
        private Texture[] textures;
        private int renderTargetToDisplayIndex;
        private Entity teapot;

        private Scene scene;

        private Entity mainCamera;

        private bool rotateModel;

        public TestMultipleRenderTargets() : this(true)
        {
        }

        public TestMultipleRenderTargets(bool rotateModel)
        {
            CurrentVersion = 4;
            this.rotateModel = rotateModel;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
            FrameGameSystem.Draw(() => ++renderTargetToDisplayIndex).TakeScreenshot();
            FrameGameSystem.Draw(() => ++renderTargetToDisplayIndex).TakeScreenshot();
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            mainCamera = new Entity
            {
                new CameraComponent
                {
                    UseCustomAspectRatio = true,
                    AspectRatio = 8/4.8f,
                    FarClipPlane = 5,
                    NearClipPlane = 1,
                    VerticalFieldOfView = MathUtil.RadiansToDegrees(0.6f),
                    UseCustomViewMatrix = true,
                    ViewMatrix = Matrix.LookAtRH(new Vector3(2,1,2), new Vector3(), Vector3.UnitY),
                },
            };
            mainCamera.Transform.Position = new Vector3(2, 1, 2);

            CreatePipeline();

            var primitive = GeometricPrimitive.Teapot.New(GraphicsDevice);
            var material = Content.Load<Material>("BasicMaterial");

            teapot = new Entity
            {
                new ModelComponent
                {
                    Model = new Model
                    {
                        material,
                        new Mesh
                        {
                            Draw = primitive.ToMeshDraw(),
                            MaterialIndex = 0,
                        }
                    }
                },
            };

            var ambientLight = new Entity("Ambient Light") { new LightComponent { Type = new LightAmbient(), Intensity = 1f } };

            scene.Entities.Add(teapot);
            scene.Entities.Add(mainCamera);
            scene.Entities.Add(ambientLight);

            // Add a custom script
            if (rotateModel)
                Script.AddTask(GameScript1);
        }

        private void CreatePipeline()
        {
            const int TargetWidth = 800;
            const int TargetHeight = 480;

            // Create render targets
            textures = new []
            {
                Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource)
            };

            var depthBuffer = Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);

            var multipleRenderFrames = new DirectRenderFrameProvider(RenderFrame.FromTexture(textures, depthBuffer));

            // Setup the default rendering pipeline
            scene = new Scene
            {
                Settings =
                {
                    GraphicsCompositor = new SceneGraphicsCompositorLayers
                    {
                        Cameras = { mainCamera.Get<CameraComponent>() },
                        ModelEffect = "MultipleRenderTargetsEffect",
                        Master =
                        {
                            Renderers =
                            {
                                new ClearRenderFrameRenderer { Color = Color.Lavender, Output = multipleRenderFrames },
                                new SceneCameraRenderer { Mode = new CameraRendererModeForward(), Output = multipleRenderFrames}, 
                                new ClearRenderFrameRenderer { Output = new MasterRenderFrameProvider() },
                                new SceneDelegateRenderer(DisplayGBuffer) { Name = "DisplayGBuffer" },
                            }
                        }
                    }
                }
            };

            SceneSystem.SceneInstance = new SceneInstance(Services, scene);
        }

        private void DisplayGBuffer(RenderDrawContext context, RenderFrame frame)
        {
            GraphicsContext.DrawTexture(textures[renderTargetToDisplayIndex]);
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                var period = (float) (2 * Math.PI * UpdateTime.Total.TotalMilliseconds / 15000);
                teapot.Transform.Rotation = Quaternion.RotationAxis(Vector3.UnitY, period);

                if (Input.PointerEvents.Any(x => x.State == PointerState.Down))
                    renderTargetToDisplayIndex = (renderTargetToDisplayIndex + 1) % 3;
            }
        }

        public static void Main()
        {
            using (var game = new TestMultipleRenderTargets())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunMultipleRenderTargets()
        {
            RunGameTest(new TestMultipleRenderTargets(false));
        }
    }
}