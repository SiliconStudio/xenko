using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
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
        // Render stages
        internal ForwardLightingRenderFeature forwardLightingRenderFeature;

        public void UpdateCameraToRenderView(RenderDrawContext context, RenderView renderView)
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

            Matrix.Multiply(ref renderView.View, ref renderView.Projection, out renderView.ViewProjection);
        }

        public void Prepare(RenderDrawContext context)
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

        public void Extract(RenderDrawContext context)
        {
            // Prepare views
            for (int index = 0; index < Views.Count; index++)
            {
                // Update indices
                var view = Views[index];
                view.Index = index;

                // Create missing RenderViewFeature
                while (view.Features.Count < RenderFeatures.Count)
                {
                    view.Features.Add(new RenderViewFeature());
                }

                for (int i = 0; i < RenderFeatures.Count; i++)
                {
                    var renderViewFeature = view.Features[i];
                    renderViewFeature.RootFeature = RenderFeatures[i];
                }
            }

            // Create nodes for objects to render
            foreach (var view in Views)
            {
                foreach (var renderObject in view.RenderObjects)
                {
                    var renderFeature = renderObject.RenderFeature;
                    var viewFeature = view.Features[renderFeature.Index];

                    // Create object node
                    renderFeature.GetOrCreateObjectNode(renderObject);

                    // Let's create the view object node
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

            // Ensure size of data arrays per objects
            PrepareDataArrays();

            // Generate and execute extract jobs
            foreach (var renderFeature in RenderFeatures)
            // We might be able to parallelize too as long as we resepect render feature dependency graph (probably very few dependencies in practice)
            {
                // Divide into task chunks for parallelism
                renderFeature.Extract();
            }

            // Ensure size of all other data arrays
            PrepareDataArrays();
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

        [DataMemberIgnore] public RenderStage MainRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage TransparentRenderStage { get; set; }
        //[DataMemberIgnore] public RenderStage GBufferRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage ShadowMapRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage PickingRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage WireFrameRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage HighlightRenderStage { get; set; }

        public bool Default { get; set; } = true;
        public bool Shadows { get; set; } = false;
        public bool GBuffer { get; set; } = false;
        public bool Picking { get; set; } = false;
        public bool WireFrame { get; set; } = false;
        public bool Highlight { get; set; } = false;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem = Context.Tags.Get(SceneInstance.CurrentRenderSystem);
            RenderContext = new RenderContext(Services);

            if (Default)
            {
                // Create mandatory render stages that don't exist yet
                if (MainRenderStage == null)
                    MainRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Main", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
                if (TransparentRenderStage == null)
                    TransparentRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Transparent", "Main", new RenderOutputDescription(GraphicsDevice.Presenter.BackBuffer.ViewFormat, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));

                // Create optional render stages that don't exist yet
                //if (GBufferRenderStage == null)
                //    GBufferRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "GBuffer", "GBuffer", new RenderOutputDescription(PixelFormat.R11G11B10_Float, GraphicsDevice.Presenter.DepthStencilBuffer.ViewFormat));
                if (Shadows && ShadowMapRenderStage == null)
                    ShadowMapRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "ShadowMapCaster", "ShadowMapCaster", new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float));
            }
            if (Picking && PickingRenderStage == null)
                PickingRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Picking", "Picking", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));
            if (WireFrame && WireFrameRenderStage == null)
                WireFrameRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "WireFrame", "WireFrame", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));
            if (Highlight && HighlightRenderStage == null)
                HighlightRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Highlight", "Highlight", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            var sceneInstance = SceneInstance.GetCurrent(Context);

            // Describe views
            mainRenderView = new RenderView();

            if (Default)
            {
                mainRenderView.RenderStages.Add(MainRenderStage);
                mainRenderView.RenderStages.Add(TransparentRenderStage);
            }

            if (PickingRenderStage != null)
                mainRenderView.RenderStages.Add(PickingRenderStage);
            if (WireFrameRenderStage != null)
                mainRenderView.RenderStages.Add(WireFrameRenderStage);
            if (HighlightRenderStage != null)
                mainRenderView.RenderStages.Add(HighlightRenderStage);

            mainRenderView.SceneInstance = sceneInstance;
            mainRenderView.SceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current);
            mainRenderView.SceneCameraSlotCollection = Context.Tags.Get(SceneCameraSlotCollection.Current);
            RenderSystem.Views.Add(mainRenderView);
        }
        
        public override void BeforeExtract(RenderContext context)
        {
            base.BeforeExtract(context);

            // Make sure required plugins are instantiated
            // TODO GRAPHICS REFACTOR this system is temporary; probably want to make it more descriptive
            if (Shadows && RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have shadows, let's enable ShadowMeshPipelinePlugin
                RenderSystem.GetPipelinePlugin<ShadowMeshPipelinePlugin>(true);
            }
            if (Picking && RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have picking, let's enable PickingMeshPipelinePlugin
                RenderSystem.GetPipelinePlugin<PickingMeshPipelinePlugin>(true);
            }
            if (WireFrame && RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have wire frame, let's enable WireFrameRenderFeature
                RenderSystem.GetPipelinePlugin<WireFrameMeshPipelinePlugin>(true);
            }
            if (Highlight && RenderSystem.GetPipelinePlugin<MeshPipelinePlugin>(false) != null)
            {
                // If MeshPipelinePlugin exists and we have wire frame, let's enable WireFrameRenderFeature
                RenderSystem.GetPipelinePlugin<HighlightMeshPipelinePlugin>(true);
            }

            // TODO: Collect shadow map views
            //RenderSystem.forwardLightingRenderFeature...

            var sceneInstance = SceneInstance.GetCurrent(Context);
            var sceneCameraRenderer = Context.Tags.Get(SceneCameraRenderer.Current);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var currentViewport = context.CommandList.Viewport;

            // GBuffer
            //if (GBuffer)
            //{
            //    context.PushRenderTargets();
            //
            //    var gbuffer = PushScopedResource(Context.Allocator.GetTemporaryTexture2D((int)currentViewport.Width, (int)currentViewport.Height, GBufferRenderStage.Output.RenderTargetFormat0));
            //    context.CommandList.Clear(gbuffer, Color4.Black);
            //    context.CommandList.SetDepthAndRenderTarget(context.CommandList.DepthStencilBuffer, gbuffer);
            //    RenderSystem.Draw(context, mainRenderView, GBufferRenderStage);
            //
            //    context.PopRenderTargets();
            //}

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

                        RenderSystem.Draw(context, shadowmapRenderView, ShadowMapRenderStage);
                    }
                }

                context.PopRenderTargets();
            }

            if (Default)
            {
                // TODO: Once there is more than one mainRenderView, shadowsRenderViews have to be rendered before their respective mainRenderView
                RenderSystem.Draw(context, mainRenderView, MainRenderStage);
                //Draw(RenderContext, mainRenderView, transparentRenderStage);
            }

            // Depth readback
            //if (Shadows)
            //{
            //    foreach (var renderer in RenderSystem.forwardLightingRenderFeature.ShadowMapRenderer.Renderers)
            //    {
            //    }
            //}

            // Material/mesh highlighting
            if (Highlight)
            {
                RenderSystem.Draw(context, mainRenderView, HighlightRenderStage);
            }

            // Wire frame
            if (WireFrame)
            {
                var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
                context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
                RenderSystem.Draw(context, mainRenderView, WireFrameRenderStage);
            }

            // Picking
            if (Picking)
            {
                if (pickingReadback == null)
                {
                    pickingReadback = ToLoadAndUnload(new ImageReadback<Vector4> { FrameDelayCount = 4 });
                    pickingTexture = Texture.New2D(GraphicsDevice, 1, 1, PickingRenderStage.Output.RenderTargetFormat0, TextureFlags.None, 1, GraphicsResourceUsage.Staging).DisposeBy(this);
                }
                var inputManager = Context.Services.GetServiceAs<InputManager>();

                // TODO: Use RenderFrame
                var pickingRenderTarget = PushScopedResource(Context.Allocator.GetTemporaryTexture2D(PickingTargetSize, PickingTargetSize, PickingRenderStage.Output.RenderTargetFormat0));
                var pickingDepthStencil = PushScopedResource(Context.Allocator.GetTemporaryTexture2D(PickingTargetSize, PickingTargetSize, PickingRenderStage.Output.DepthStencilFormat, TextureFlags.DepthStencil));

                // Render the picking stage using the current view
                context.PushRenderTargets();
                { 
                    context.CommandList.Clear(pickingRenderTarget, Color.Transparent);
                    context.CommandList.Clear(pickingDepthStencil, DepthStencilClearOptions.DepthBuffer);

                    context.CommandList.SetDepthAndRenderTarget(pickingDepthStencil, pickingRenderTarget);
                    RenderSystem.Draw(context, mainRenderView, PickingRenderStage);
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