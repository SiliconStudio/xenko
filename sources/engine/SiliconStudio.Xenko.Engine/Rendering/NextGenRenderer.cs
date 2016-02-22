using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Background;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Rendering.Editor;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Rendering
{
    public partial class NextGenRenderSystem
    {
        internal RenderStage mainRenderStage = new RenderStage("Main");
        internal RenderStage transparentRenderStage = new RenderStage("Main");
        internal RenderStage gbufferRenderStage = new RenderStage("GBuffer");
        internal RenderStage shadowmapRenderStage = new RenderStage("ShadowMapCaster");
        internal RenderStage pickingRenderStage = new RenderStage("Picking");

        // Render stages
        internal ForwardLightingRenderFeature forwardLightingRenderFeature;

        public void ExtractAndPrepare(RenderDrawContext context)
        {
            // Update current camera to render view
            foreach (var mainRenderView in Views)
            {
                if (mainRenderView.GetType() == typeof(RenderView))
                {
                    UpdateCameraToRenderView(context, mainRenderView);
                }
            }

            // Extract data from the scene
            Extract(context);

            // Perform most of computations
            Prepare(context);
        }

        private void UpdateCameraToRenderView(RenderDrawContext context, RenderView renderView)
        {
            renderView.Camera = renderView.SceneCameraSlotCollection.GetCamera(renderView.SceneCameraRenderer.Camera);

            if (renderView.Camera == null)
                return;

            // Setup viewport size
            var currentViewport = renderView.SceneCameraRenderer.ComputedViewport;
            var aspectRatio = currentViewport.AspectRatio;

            // Update the aspect ratio
            if (renderView.Camera.UseCustomAspectRatio)
            {
                aspectRatio = renderView.Camera.AspectRatio;
            }

            // If the aspect ratio is calculated automatically from the current viewport, update matrices here
            renderView.Camera.Update(aspectRatio);

            renderView.View = renderView.Camera.ViewMatrix;
            renderView.Projection = renderView.Camera.ProjectionMatrix;
        }

        private void Prepare(RenderDrawContext context)
        {
            // Sync point: after extract, before prepare (game simulation could resume now)

            // Generate and execute prepare effect jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.PrepareEffectPermutations();
            }

            // Generate and execute prepare jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Prepare(context.RenderContext);
            }
        }

        private void Extract(RenderDrawContext context)
        {
            // Reset render context data
            Reset();

            // Create object nodes
            foreach (var renderObject in RenderObjects)
            {
                var renderFeature = renderObject.RenderFeature;
                renderFeature.GetOrCreateObjectNode(renderObject);
            }

            // Ensure size of data arrays per objects
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareDataArrays();
            }

            // Generate and execute extract jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Extract();
            }

            // Reset view specific render context data
            ResetViews();

            // Collect objects to render (later we will also cull/filter them)
            foreach (var view in Views)
            {
                // TODO: Culling & filtering
                foreach (var renderObject in RenderObjects)
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
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareDataArrays();
            }
        }

        public void Draw(RenderDrawContext renderDrawContext, RenderView renderView, RenderStage renderStage)
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
                currentRenderFeature.Draw(renderDrawContext, renderView, renderViewStage, currentStart, currentEnd);
            }
        }
    }

    [DataContract("NextGenRenderer")]
    public class NextGenRenderer : CameraRendererMode
    {
        [DataMemberIgnore]
        public NextGenRenderSystem RenderSystem;
        [DataMemberIgnore]
        public RenderContext RenderContext;

        // Render views
        private RenderView mainRenderView;

        private ForwardLightingRenderFeature forwardLightingRenderFeasture;

        public override string ModelEffect { get; set; }

        public bool Shadows { get; set; } = false;
        public bool GBuffer { get; set; } = false;
        public bool Picking { get; set; } = false;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem = Services.GetServiceAs<NextGenRenderSystem>();
            if (RenderSystem == null)
            {
                RenderSystem = new NextGenRenderSystem(Services);

                RenderSystem.Initialize(EffectSystem, GraphicsDevice);

                // Setup stage targets
                RenderSystem.mainRenderStage.Output = new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat);
                RenderSystem.transparentRenderStage.Output = new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat);
                RenderSystem.gbufferRenderStage.Output = new RenderOutputDescription(PixelFormat.R11G11B10_Float, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat);
                RenderSystem.shadowmapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);

                RenderSystem.RenderStages.Add(RenderSystem.mainRenderStage);
                RenderSystem.RenderStages.Add(RenderSystem.transparentRenderStage);
                RenderSystem.RenderStages.Add(RenderSystem.gbufferRenderStage);
                RenderSystem.RenderStages.Add(RenderSystem.shadowmapRenderStage);
                RenderSystem.RenderStages.Add(RenderSystem.pickingRenderStage);

                var meshRenderFeature = new MeshRenderFeature
                {
                    RenderFeatures =
                    {
                        new TransformRenderFeature(),
                        //new SkinningRenderFeature(),
                        new MaterialRenderFeature(),
                        (RenderSystem.forwardLightingRenderFeature = new ForwardLightingRenderFeature { ShadowmapRenderStage = RenderSystem.shadowmapRenderStage }),
                        new PickingRenderFeature(),
                    },
                };

                meshRenderFeature.PostProcessPipelineState += (RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState) =>
                {
                    if (renderNode.RenderStage == RenderSystem.shadowmapRenderStage)
                    {
                        pipelineState.RasterizerState = new RasterizerStateDescription(CullMode.None) { DepthClipEnable = false };
                    }
                };

                meshRenderFeature.ComputeRenderStages += (renderObject) =>
                {
                    var renderMesh = (RenderMesh)renderObject;

                    // Main or transparent pass
                    if (renderMesh.Material.Material.HasTransparency)
                    {
                        renderMesh.ActiveRenderStages[RenderSystem.transparentRenderStage.Index] = new ActiveRenderStage("TestEffect");
                    }
                    else
                    {
                        renderMesh.ActiveRenderStages[RenderSystem.mainRenderStage.Index] = new ActiveRenderStage("TestEffect");

                        // Also enable shadow and gbuffer rendering if activated
                        if (/*Shadows && */renderMesh.Material.IsShadowCaster)
                            renderMesh.ActiveRenderStages[RenderSystem.shadowmapRenderStage.Index] = new ActiveRenderStage("TestEffect.ShadowMapCaster");

                        if (GBuffer)
                            renderMesh.ActiveRenderStages[RenderSystem.gbufferRenderStage.Index] = new ActiveRenderStage("TestEffect.GBuffer");
                    }

                    //if (Picking)
                        renderMesh.ActiveRenderStages[RenderSystem.pickingRenderStage.Index] = new ActiveRenderStage("TestEffect.Picking");
                };

                var spriteRenderFeature = new SpriteRenderFeature();
                spriteRenderFeature.ComputeRenderStages += renderObject =>
                {
                    var renderSprite = (RenderSprite)renderObject;

                    if (renderSprite.SpriteComponent.CurrentSprite.IsTransparent)
                        renderSprite.ActiveRenderStages[RenderSystem.transparentRenderStage.Index] = new ActiveRenderStage("Test");
                    else
                        renderSprite.ActiveRenderStages[RenderSystem.mainRenderStage.Index] = new ActiveRenderStage("Test");
                };

                var skyboxRenderFeature = new SkyboxRenderFeature();
                skyboxRenderFeature.ComputeRenderStages += renderObject =>
                {
                    renderObject.ActiveRenderStages[RenderSystem.mainRenderStage.Index] = new ActiveRenderStage("SkyboxEffect");
                };

                var backgroundFeature = new BackgroundRenderFeature();
                backgroundFeature.ComputeRenderStages += renderObject =>
                {
                    renderObject.ActiveRenderStages[RenderSystem.mainRenderStage.Index] = new ActiveRenderStage("Test");
                };

                // Register top level renderers
                RenderSystem.RenderFeatures.Add(meshRenderFeature);
                RenderSystem.RenderFeatures.Add(spriteRenderFeature);
                RenderSystem.RenderFeatures.Add(skyboxRenderFeature);
                RenderSystem.RenderFeatures.Add(backgroundFeature);

                RenderSystem.InitializeFeatures();
            }

            RenderContext = new RenderContext(Services);

            // Attach model processor (which will register meshes to render system)
            var sceneInstance = SceneInstance.GetCurrent(Context);
            sceneInstance.Processors.Add(new NextGenModelProcessor());
            sceneInstance.Processors.Add(new NextGenSpriteProcessor());
            sceneInstance.Processors.Add(new NextGenBackgroundProcessor());
            sceneInstance.Processors.Add(new NextGenSkyboxProcessor());

            // Describe views
            mainRenderView = new RenderView { RenderStages = { RenderSystem.mainRenderStage, RenderSystem.transparentRenderStage, RenderSystem.gbufferRenderStage, RenderSystem.pickingRenderStage } };
            mainRenderView.SceneInstance = sceneInstance;
            mainRenderView.SceneCameraRenderer = RenderSystem.RenderContextOld.Tags.Get(SceneCameraRenderer.Current);
            mainRenderView.SceneCameraSlotCollection = RenderSystem.RenderContextOld.Tags.Get(SceneCameraSlotCollection.Current);
            RenderSystem.Views.Add(mainRenderView);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var currentViewport = context.CommandList.Viewport;

            // GBuffer
            if (GBuffer)
            {
                context.PushRenderTargets();

                var gbuffer = PushScopedResource(Context.Allocator.GetTemporaryTexture2D((int)currentViewport.Width, (int)currentViewport.Height, PixelFormat.R11G11B10_Float));
                context.CommandList.Clear(gbuffer, Color4.Black);
                context.CommandList.SetDepthAndRenderTarget(context.CommandList.DepthStencilBuffer, gbuffer);
                RenderSystem.Draw(context, mainRenderView, RenderSystem.gbufferRenderStage);

                context.PopRenderTargets();
            }

            // Shadow maps
            if (Shadows)
            {
                // Clear atlases
                RenderSystem.forwardLightingRenderFeature.ShadowMapRenderer.ClearAtlasRenderTargets(context.CommandList);

                context.PushRenderTargets();

                // Draw all shadow views generated for the current view
                foreach (var renderView in RenderSystem.Views)
                {
                    var shadowmapRenderView = renderView as ShadowMapRenderView;
                    if (shadowmapRenderView != null && shadowmapRenderView.RenderView == mainRenderView)
                    {
                        var shadowMapRectangle = shadowmapRenderView.Rectangle;
                        shadowmapRenderView.ShadowMapTexture.Atlas.RenderFrame.Activate(context);
                        shadowmapRenderView.ShadowMapTexture.Atlas.MarkClearNeeded();
                        context.CommandList.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                        RenderSystem.Draw(context, shadowmapRenderView, RenderSystem.shadowmapRenderStage);
                    }
                }

                context.PopRenderTargets();
            }

            // TODO: Once there is more than one mainRenderView, shadowsRenderViews have to be rendered before their respective mainRenderView
            RenderSystem.Draw(context, mainRenderView, RenderSystem.mainRenderStage);
            //Draw(RenderContext, mainRenderView, transparentRenderStage);

            // Depth readback
            //if (Shadows)
            //{
            //    foreach (var renderer in RenderSystem.forwardLightingRenderFeature.ShadowMapRenderer.Renderers)
            //    {
            //    }
            //}

            // Picking
            if (Picking)
            {
                if (pickingReadback == null)
                {
                    pickingReadback = ToLoadAndUnload(new ImageReadback<Vector4> { FrameDelayCount = 4 });
                    pickingTexture = Texture.New2D(GraphicsDevice, 1, 1, PixelFormat.R32G32B32A32_Float, TextureFlags.None, 1, GraphicsResourceUsage.Staging).DisposeBy(this);
                }
                var inputManager = Context.Services.GetServiceAs<InputManager>();

                // TODO: Use RenderFrame
                var pickingRenderTarget = PushScopedResource(Context.Allocator.GetTemporaryTexture2D(PickingTargetSize, PickingTargetSize, PixelFormat.R32G32B32A32_Float));
                var pickingDepthStencil = PushScopedResource(Context.Allocator.GetTemporaryTexture2D(PickingTargetSize, PickingTargetSize, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil));

                // Render the picking stage using the current view
                context.PushRenderTargets();
                { 
                    context.CommandList.Clear(pickingRenderTarget, Color.Transparent);
                    context.CommandList.Clear(pickingDepthStencil, DepthStencilClearOptions.DepthBuffer);

                    context.CommandList.SetDepthAndRenderTarget(pickingDepthStencil, pickingRenderTarget);
                    RenderSystem.Draw(context, mainRenderView, RenderSystem.pickingRenderStage);
                }
                context.PopRenderTargets();

                // Copy the correct texel and read it back
                // TODO: We could just render the scene to the single texel being picked
                CopyPicking(context, pickingRenderTarget, inputManager.MousePosition);
                pickingReadback.SetInput(pickingTexture);
                pickingReadback.Draw(context);

                // Result should be used during extract phase
                if (pickingReadback.IsResultAvailable)
                {
                    var encodedResult = pickingReadback.Result[0];
                    unsafe
                    {
                        pickingResult = *(Int3*)&encodedResult;
                    }
                }
            }
        }

        private const int PickingTargetSize = 512;

        private Int3 pickingResult;
        private readonly Dictionary<int, Entity> idToEntity = new Dictionary<int, Entity>();
        private ImageReadback<Vector4> pickingReadback;
        private Texture pickingTexture;

        private void CopyPicking(RenderDrawContext context, Texture pickingRenderTarget, Vector2 mousePosition)
        {
            var renderTargetSize = new Vector2(pickingRenderTarget.Width, pickingRenderTarget.Height);
            var positionInTexture = Vector2.Modulate(renderTargetSize, mousePosition);
            var region = new ResourceRegion(
                Math.Max(0, Math.Min((int)renderTargetSize.X - 1, (int)positionInTexture.X)),
                Math.Max(0, Math.Min((int)renderTargetSize.Y - 1, (int)positionInTexture.Y)),
                0,
                Math.Max(0, Math.Min((int)renderTargetSize.X - 1, (int)positionInTexture.X + 1)),
                Math.Max(0, Math.Min((int)renderTargetSize.Y - 1, (int)positionInTexture.Y + 1)),
                1);

            // Copy results to 1x1 target
            context.CommandList.CopyRegion(pickingRenderTarget, 0, region, pickingTexture, 0);
        }

        /// <summary>
        /// Cache all the components ids in the <see cref="idToEntity"/> dictionary.
        /// </summary>
        /// <param name="componentBase">the component to tag recursively.</param>
        /// <param name="isRecursive">indicate if cache should be built recursively</param>
        public void CacheComponentsId(ComponentBase componentBase, bool isRecursive)
        {
            var scene = componentBase as Scene;
            if (scene != null && isRecursive)
            {
                foreach (var entity in scene.Entities)
                    CacheComponentsId(entity, true);
            }
            else
            {
                var entity = componentBase as Entity;
                if (entity != null)
                {
                    foreach (var component in entity.Components)
                        idToEntity[RuntimeIdHelper.ToRuntimeId(component)] = entity;

                    if (isRecursive)
                    {
                        foreach (var child in entity.Transform.Children)
                            CacheComponentsId(child.Entity, true);
                    }
                }
            }
        }

        /// <summary>
        /// Uncache all the components ids in the <see cref="idToEntity"/> dictionary.
        /// </summary>
        /// <param name="entity">the entity to tag recursively.</param>
        /// <param name="isReccursive">indicate if cache should be built recursively</param>
        public void UncacheComponentsId(Entity entity, bool isReccursive)
        {
            foreach (var component in entity.Components)
            {
                var runtimeId = RuntimeIdHelper.ToRuntimeId(component);
                if (idToEntity.ContainsKey(runtimeId))
                    idToEntity.Remove(runtimeId);
            }

            if (isReccursive)
            {
                foreach (var child in entity.Transform.Children)
                    UncacheComponentsId(child.Entity, true);
            }
        }
        /// <summary>
        /// Gets the entity at the provided screen position
        /// </summary>
        /// <returns></returns>
        public EntityPickingResult Pick()
        {
            var result = new EntityPickingResult
            {
                ComponentId = pickingResult.X,
                MeshNodeIndex = pickingResult.Y,
                MaterialIndex = pickingResult.Z
            };
            result.Entity = idToEntity.ContainsKey(result.ComponentId) ? idToEntity[result.ComponentId] : null;
            Debug.WriteLine(result);
            return result;
        }
    }
}