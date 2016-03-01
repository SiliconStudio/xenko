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
using SiliconStudio.Xenko.Rendering.Editor;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Rendering
{
    [DataContract("CameraRendererModeForward")]
    public class CameraRendererModeForward : CameraRenderModeBase
    {
        private ForwardLightingRenderFeature forwardLightingRenderFeasture;

        [DataMemberIgnore] public RenderStage MainRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage TransparentRenderStage { get; set; }
        //[DataMemberIgnore] public RenderStage GBufferRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage ShadowMapRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage PickingRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage WireFrameRenderStage { get; set; }
        [DataMemberIgnore] public RenderStage HighlightRenderStage { get; set; }

        public bool Shadows { get; set; } = true;
        public bool GBuffer { get; set; } = false;
        public bool Picking { get; set; } = false;
        public bool WireFrame { get; set; } = false;
        public bool Highlight { get; set; } = false;

        protected override void InitializeCore()
        {
            base.InitializeCore();

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
            if (Picking && PickingRenderStage == null)
                PickingRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Picking", "Picking", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));
            if (WireFrame && WireFrameRenderStage == null)
                WireFrameRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "WireFrame", "WireFrame", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));
            if (Highlight && HighlightRenderStage == null)
                HighlightRenderStage = EntityComponentRendererBase.GetOrCreateRenderStage(RenderSystem, "Highlight", "Highlight", new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            MainRenderView.RenderStages.Add(MainRenderStage);
            MainRenderView.RenderStages.Add(TransparentRenderStage);
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

            // TODO GRAPHICS REFACTOR: Make this non-explicit?
            RenderSystem.forwardLightingRenderFeature?.BeforeExtract();
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
                RenderSystem.forwardLightingRenderFeature?.ShadowMapRenderer.ClearAtlasRenderTargets(context.CommandList);

                context.PushRenderTargets();

                // Draw all shadow views generated for the current view
                foreach (var renderView in RenderSystem.Views)
                {
                    var shadowmapRenderView = renderView as ShadowMapRenderView;
                    if (shadowmapRenderView != null && shadowmapRenderView.RenderView == MainRenderView)
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

            RenderSystem.Draw(context, MainRenderView, MainRenderStage);
            RenderSystem.Draw(context, MainRenderView, TransparentRenderStage);

            // Material/mesh highlighting
            if (Highlight)
            {
                RenderSystem.Draw(context, MainRenderView, HighlightRenderStage);
            }

            // Wire frame
            if (WireFrame)
            {
                var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
                context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
                RenderSystem.Draw(context, MainRenderView, WireFrameRenderStage);
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
                    RenderSystem.Draw(context, MainRenderView, PickingRenderStage);
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
            return result;
        }
    }
}