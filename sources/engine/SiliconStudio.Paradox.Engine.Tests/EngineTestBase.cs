// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;
using SiliconStudio.Paradox.Graphics.Regression;

namespace SiliconStudio.Paradox.Engine.Tests
{
    /// <summary>
    /// Base class for engine tests.
    /// </summary>
    public class EngineTestBase : GraphicsTestBase
    {
        protected Scene Scene;
        protected Entity Camera = new Entity { new CameraComponent() };

        protected CameraComponent CameraComponent
        {
            get {  return Camera.Get<CameraComponent>(); }
            set
            {
                Camera.Add(value);
                graphicsCompositor.Cameras[0] = value;
            }
        }

        protected readonly CameraRendererModeForward SceneCameraRenderer;

        private SceneGraphicsCompositorLayers graphicsCompositor;

        public EngineTestBase(string effectName = "ParadoxEditorForwardShadingEffect")
        {
            SceneCameraRenderer = new CameraRendererModeForward { Name = "Camera renderer", ModelEffect = effectName };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            graphicsCompositor = new SceneGraphicsCompositorLayers
            {
                Cameras = { Camera.Get<CameraComponent>() },
                Master =
                {
                    Renderers =
                    {
                        new ClearRenderFrameRenderer { Color = Color.Green, Name = "Clear frame" },
                        new SceneDelegateRenderer(PreCameraRendererDraw),
                        new SceneCameraRenderer { Mode = SceneCameraRenderer },
                        new SceneDelegateRenderer(PostCameraRendererDraw),
                    }
                }
            };

            Scene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };
            Scene.AddChild(Camera);

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);
        }

        protected virtual void PreCameraRendererDraw(RenderContext context, RenderFrame frame)
        {
            
        }

        protected virtual void PostCameraRendererDraw(RenderContext context, RenderFrame frame)
        {
        }
    }
}