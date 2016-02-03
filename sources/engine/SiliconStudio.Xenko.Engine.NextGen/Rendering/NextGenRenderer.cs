using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Rendering
{
    public class NextGenRenderer : CameraRendererMode
    {
        //public RendererBase RootRenderer;
        public NextGenRenderSystem RenderSystem;
        public NextGenRenderContext RenderContext;

        // Render views
        private RenderView mainRenderView;

        // Render stages
        private RenderStage mainRenderStage = new RenderStage("Main");
        private RenderStage transparentRenderStage = new RenderStage("Main");
        private RenderStage gbufferRenderStage = new RenderStage("GBuffer");
        private RenderStage shadowmapRenderStage = new RenderStage("ShadowMapCaster");

        private double time;

        private RasterizerState shadowMapState;

        public override string ModelEffect { get; set; }

        public bool Shadows { get; set; } = true;
        public bool GBuffer { get; set; } = false;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem = new NextGenRenderSystem(Services);
            RenderContext = new NextGenRenderContext(GraphicsDevice);

            RenderSystem.Initialize(EffectSystem, GraphicsDevice);

            RenderSystem.RenderStages.Add(mainRenderStage);
            RenderSystem.RenderStages.Add(transparentRenderStage);
            RenderSystem.RenderStages.Add(gbufferRenderStage);
            RenderSystem.RenderStages.Add(shadowmapRenderStage);

            // Describe views
            mainRenderView = new RenderView { RenderStages = { mainRenderStage, transparentRenderStage, gbufferRenderStage } };
            var meshRenderFeature = new MeshRenderFeature
            {
                RenderFeatures =
                    {
                        new TransformRenderFeature(),
                        //new SkinningRenderFeature(),
                        new MaterialRenderFeature(),
                        new ForwardLightingRenderFeature() { ShadowmapRenderStage = shadowmapRenderStage } ,
                    },
            };

            meshRenderFeature.ComputeRenderStages += (renderObject) =>
            {
                var renderMesh = (RenderMesh)renderObject;

                // Main or transparent pass
                if (renderMesh.Material.Material.HasTransparency)
                {
                    renderMesh.ActiveRenderStages[transparentRenderStage.Index] = new ActiveRenderStage("TestEffect");
                }
                else
                {
                    renderMesh.ActiveRenderStages[mainRenderStage.Index] = new ActiveRenderStage("TestEffect");

                    // Also enable shadow and gbuffer rendering if activated
                    if (Shadows && renderMesh.Material.IsShadowCaster)
                        renderMesh.ActiveRenderStages[shadowmapRenderStage.Index] = new ActiveRenderStage("TestEffect.ShadowMapCaster");

                    if (GBuffer)
                        renderMesh.ActiveRenderStages[gbufferRenderStage.Index] = new ActiveRenderStage("TestEffect.GBuffer");
                }
            };

            var spriteRenderFeature = new SpriteRenderFeature();
            spriteRenderFeature.ComputeRenderStages += renderObject =>
            {
                var renderSprite = (RenderSprite)renderObject;

                if (renderSprite.SpriteComponent.CurrentSprite.IsTransparent)
                    renderSprite.ActiveRenderStages[transparentRenderStage.Index] = new ActiveRenderStage("Test");
                else
                    renderSprite.ActiveRenderStages[mainRenderStage.Index] = new ActiveRenderStage("Test");
            };

            var skyboxRenderFeature = new SkyboxRenderFeature();
            skyboxRenderFeature.ComputeRenderStages += renderObject =>
            {
                renderObject.ActiveRenderStages[mainRenderStage.Index] = new ActiveRenderStage("SkyboxEffect");
            };

            // Register top level renderers
            RenderSystem.RenderFeatures.Add(meshRenderFeature);
            RenderSystem.RenderFeatures.Add(spriteRenderFeature);
            RenderSystem.RenderFeatures.Add(skyboxRenderFeature);

            RenderSystem.Views.Add(mainRenderView);

            RenderSystem.Initialize();

            // Attach model processor (which will register meshes to render system)
            var sceneInstance = SceneInstance.GetCurrent(Context);
            sceneInstance.Processors.Add(new NextGenModelProcessor());
            sceneInstance.Processors.Add(new NextGenSpriteProcessor());
            sceneInstance.Processors.Add(new SkyboxProcessor());
        }

        protected override void DrawCore(RenderContext context)
        {
            // Move viewpoint
            // TODO: Use camera system and Renderer
            time += EffectSystem.Game.DrawTime.Elapsed.TotalSeconds;

            // Update current camera to render view
            UpdateCameraToRenderView(context, context.GetCurrentCamera(), mainRenderView);

            // Extract data from the scene
            Extract(context);

            // Perform most of computations
            Prepare();

            var currentViewport = context.GraphicsDevice.Viewport;

            // GBuffer
            if (GBuffer)
            {
                GraphicsDevice.PushState();

                var gbuffer = PushScopedResource(Context.Allocator.GetTemporaryTexture2D((int)currentViewport.Width, (int)currentViewport.Height, PixelFormat.R11G11B10_Float));
                GraphicsDevice.Clear(gbuffer, Color4.Black);
                GraphicsDevice.SetDepthAndRenderTarget(GraphicsDevice.DepthStencilBuffer, gbuffer);
                Draw(RenderSystem, RenderContext, mainRenderView, gbufferRenderStage);
                GraphicsDevice.PopState();
            }

            // Shadow maps
            // TODO: Move that to a class that will handle all the details of shadow mapping
            if (Shadows)
            {
                if (shadowMapState == null)
                    shadowMapState = RasterizerState.New(GraphicsDevice, new RasterizerStateDescription(CullMode.None) { DepthClipEnable = false });

                GraphicsDevice.PushState();
                foreach (var renderView in RenderSystem.Views)
                {
                    var shadowmapRenderView = renderView as ShadowMapRenderView;
                    if (shadowmapRenderView != null)
                    {
                        var shadowMapRectangle = shadowmapRenderView.Rectangle;
                        shadowmapRenderView.ShadowMapTexture.Atlas.RenderFrame.Activate(context);
                        GraphicsDevice.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));
                        GraphicsDevice.SetRasterizerState(shadowMapState);

                        Draw(RenderSystem, RenderContext, shadowmapRenderView, shadowmapRenderStage);
                    }
                }
                GraphicsDevice.PopState();
            }


            Draw(RenderSystem, RenderContext, mainRenderView, mainRenderStage);
            //Draw(RenderContext, mainRenderView, transparentRenderStage);
        }

        private void UpdateCameraToRenderView(RenderContext context, CameraComponent camera, RenderView renderView)
        {
            // Setup viewport size
            var currentViewport = context.GraphicsDevice.Viewport;
            var aspectRatio = currentViewport.AspectRatio;

            // Update the aspect ratio
            if (camera.UseCustomAspectRatio)
            {
                aspectRatio = camera.AspectRatio;
            }

            // If the aspect ratio is calculated automatically from the current viewport, update matrices here
            camera.Update(aspectRatio);

            renderView.View = camera.ViewMatrix;
            renderView.Projection = camera.ProjectionMatrix;
        }

        private void Extract(RenderContext context)
        {
            var sceneInstance = SceneInstance.GetCurrent(context);
            
            // Reset render context data
            RenderSystem.Reset();

            // Create object nodes
            foreach (var renderObject in RenderSystem.RenderObjects)
            {
                var renderFeature = renderObject.RenderFeature;
                renderFeature.GetOrCreateObjectNode(renderObject);
            }

            // Ensure size of data arrays per objects
            foreach (var renderFeature in RenderSystem.RenderFeatures)
            {
                renderFeature.PrepareDataArrays(RenderSystem);
            }

            // Generate and execute extract jobs
            foreach (var renderFeature in RenderSystem.RenderFeatures)
                // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Extract();
            }

            // Reset view specific render context data
            RenderSystem.ResetViews();

            // Collect objects to render (later we will also cull/filter them)
            foreach (var view in RenderSystem.Views)
            {
                // TODO: Culling & filtering
                foreach (var renderObject in RenderSystem.RenderObjects)
                {
                    var viewFeature = view.Features[renderObject.RenderFeature.Index];

                    var renderFeature = renderObject.RenderFeature;

                    var renderViewNode = renderFeature.CreateViewObjectNode(view, renderObject);
                    viewFeature.ViewObjectNodes.Add(renderViewNode);

                    // Collect object
                    // TODO: Check which stage it belongs to (and skip everything if it doesn't belong to any stage)
                    // TODO: For now, we build list and then copy. Another way would be to count and then fill (might be worse, need to check)
                    var activeRenderStages = renderObject.ActiveRenderStages;
                    foreach (var renderViewStage in view.RenderStages)
                    {
                        // Check if this RenderObject wants to be rendered for this render stage
                        var renderStageIndex = renderViewStage.RenderStage.Index;
                        if (!activeRenderStages[renderStageIndex].Active)
                            continue;

                        var renderNode = renderFeature.CreateRenderNode(renderObject, view, renderViewNode, renderViewStage.RenderStage);

                        // Note: Used mostly during updating
                        viewFeature.RenderNodes.Add(renderNode);

                        // Note: Used mostly during rendering
                        renderViewStage.RenderNodes.Add(new RenderNodeFeatureReference(renderFeature, renderNode));
                    }
                }

                // TODO: Sort RenderStage.RenderNodes
            }

            // Ensure size of all other data arrays
            foreach (var renderFeature in RenderSystem.RenderFeatures)
            {
                renderFeature.PrepareDataArrays(RenderSystem);
            }
        }

        private void Prepare()
        {
            // Sync point: after extract, before prepare (game simulation could resume now)

            // Generate and execute prepare effect jobs
            foreach (var renderFeature in RenderSystem.RenderFeatures)
                // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.PrepareEffectPermutations();
            }

            // Generate and execute prepare jobs
            foreach (var renderFeature in RenderSystem.RenderFeatures)
                // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Prepare(RenderContext);
            }
        }

        public static void Draw(NextGenRenderSystem renderSystem, NextGenRenderContext renderContext, RenderView renderView, RenderStage renderStage)
        {
            // Sync point: draw (from now, we should execute with a graphics device context to perform rendering)

            // Look for the RenderViewStage corresponding to this RenderView | RenderStage combination
            RenderViewStage renderViewStage = null;
            foreach (var currentRenderViewStage in renderView.RenderStages)
            {
                if (currentRenderViewStage.RenderStage == renderStage)
                {
                    renderViewStage = currentRenderViewStage;
                    break;
                }
            }

            if (renderViewStage == null)
            {
                throw new InvalidOperationException("Requested RenderView|RenderStage combination doesn't exist. Please add it to RenderView.RenderStages.");
            }

            // Generate and execute draw jobs
            var renderNodes = renderViewStage.RenderNodes;
            int currentStart, currentEnd;

            for (currentStart = 0; currentStart < renderNodes.Count; currentStart = currentEnd)
            {
                var currentRenderFeature = renderNodes[currentStart].RootRenderFeature;
                currentEnd = currentStart + 1;
                while (currentEnd < renderNodes.Count && renderNodes[currentEnd].RootRenderFeature == currentRenderFeature)
                {
                    currentEnd++;
                }

                // Divide into task chunks for parallelism
                currentRenderFeature.Draw(renderContext, renderView, renderViewStage, currentStart, currentEnd);
            }
        }
    }
}