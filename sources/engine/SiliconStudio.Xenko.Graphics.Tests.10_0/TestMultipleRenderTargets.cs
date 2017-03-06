// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;
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

            textures = new[]
            {
                Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource),
                Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource)
            };

            // Setup the default rendering pipeline
            scene = new Scene();
            SceneSystem.SceneInstance = new SceneInstance(Services, scene);

            SceneSystem.GraphicsCompositor = GraphicsCompositor.CreateDefault(false, "MultipleRenderTargetsEffect", mainCamera.Get<CameraComponent>(), Color.Lavender);
            SceneSystem.GraphicsCompositor.Game = new SceneRendererCollection
            {
                new RenderTargetRenderer
                {
                    Child = SceneSystem.GraphicsCompositor.Game,
                    RenderTargets = { textures[0], textures[1], textures[2] },
                    DepthStencil = Texture.New2D(GraphicsDevice, TargetWidth, TargetHeight, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil),
                },
                new ClearRenderer(),
                new DelegateSceneRenderer(DisplayGBuffer),
            };
        }

        private void DisplayGBuffer(RenderDrawContext context)
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

                if (Input.PointerEvents.Any(x => x.EventType == PointerEventType.Pressed))
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

    // TODO: Maybe we could move this type and make it public at some point? (and maybe merge it with RenderTextureSceneRenderer)
    class RenderTargetRenderer : SceneRendererBase
    {
        public FastList<Texture> RenderTargets { get; set; } = new FastList<Texture>();

        public Texture DepthStencil { get; set; }

        public ISceneRenderer Child { get; set; }

        protected override unsafe void CollectCore(RenderContext context)
        {
            base.CollectCore(context);

            var firstTexture = DepthStencil ?? (RenderTargets.Count > 0 ? RenderTargets[0] : null);
            if (firstTexture == null)
                return;

            using (context.SaveRenderOutputAndRestore())
            using (context.SaveViewportAndRestore())
            {
                context.RenderOutput.RenderTargetCount = RenderTargets.Count;
                fixed (PixelFormat* renderTargetFormat0 = &context.RenderOutput.RenderTargetFormat0)
                {
                    var renderTargetFormat = renderTargetFormat0;
                    for (int i = 0; i < RenderTargets.Count; ++i)
                    {
                        *renderTargetFormat++ = RenderTargets[i].ViewFormat;
                    }
                }
                context.RenderOutput.DepthStencilFormat = DepthStencil.ViewFormat;

                context.ViewportState.Viewport0 = new Viewport(0, 0, firstTexture.ViewWidth, firstTexture.ViewHeight);

                Child?.Collect(context);
            }
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var firstTexture = DepthStencil ?? (RenderTargets.Count > 0 ? RenderTargets[0] : null);
            if (firstTexture == null)
                return;

            using (drawContext.PushRenderTargetsAndRestore())
            {
                drawContext.CommandList.SetRenderTargetsAndViewport(DepthStencil, RenderTargets.Count, RenderTargets.Items);

                Child?.Draw(drawContext);
            }
        }

    }
}