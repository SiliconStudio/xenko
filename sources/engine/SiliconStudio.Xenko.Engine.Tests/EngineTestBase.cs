// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Engine.Tests
{
    /// <summary>
    /// Base class for engine tests.
    /// </summary>
    public class EngineTestBase : GameTestBase
    {
        protected Scene Scene;
        protected Entity Camera = new Entity { new CameraComponent() };
        protected LightComponent AmbientLight;

        protected CameraComponent CameraComponent
        {
            get {  return Camera.Get<CameraComponent>(); }
            set
            {
                bool alreadyAdded = false;
                for (int i = 0; i < Camera.Components.Count; i++)
                {
                    var component = Camera.Components[i];
                    if (component == value)
                    {
                        alreadyAdded = true;
                        break;
                    }
                    if (component is CameraComponent)
                    {
                        alreadyAdded = true;
                        Camera.Components[i] = value;
                        break;
                    }
                }
                if (!alreadyAdded)
                {
                    Camera.Add(value);
                }
                graphicsCompositor.Cameras[0] = value;
            }
        }

        protected readonly CameraRendererModeForward SceneCameraRenderer;

        private SceneGraphicsCompositorLayers graphicsCompositor;

        public EngineTestBase()
        {
            SceneCameraRenderer = new CameraRendererModeForward { Name = "Camera renderer" };
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
            Scene.Entities.Add(Camera);

            AmbientLight = new LightComponent { Type = new LightAmbient { Color = new ColorRgbProvider(Color.White) }, Intensity = 1 };
            var ambientLight = new Entity { AmbientLight };
            Scene.Entities.Add(ambientLight);

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);
        }

        protected virtual void PreCameraRendererDraw(RenderDrawContext context, RenderFrame frame)
        {
            
        }

        protected virtual void PostCameraRendererDraw(RenderDrawContext context, RenderFrame frame)
        {
        }
    }
}