// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    public partial class EditorCompositor : GraphicsCompositorTopPart, IGraphicsCompositorSharedPart
    {
        public TopLevelCompositor ContentCompositor { get; set; }

        public override void Collect(RenderContext context)
        {
            base.Collect(context);

            // Collect gizmos
            ContentCompositor.MainRenderView.RenderStages.Add(GizmoRenderStage);
            ContentCompositor.Collect(context);
        }

        public override void Draw(RenderDrawContext context)
        {
            base.Draw(context);
            ContentCompositor.Draw(context);

            context.RenderContext.RenderSystem.Draw(context, ContentCompositor.MainRenderView, GizmoRenderStage);
        }

        public RenderStage GizmoRenderStage { get; set; }
    }

    // Note: Kept in a single file for easy iteration until the graphics compositor refactor is finished
    public partial class TopLevelCompositor : GraphicsCompositorTopPart, IGraphicsCompositorSharedPart
    {
        public IGraphicsCompositorViewPart UnitRenderer { get; set; }

        [DataMemberIgnore]
        public RenderView MainRenderView { get; } = new RenderView();

        public PostProcessingEffects PostEffects { get; set; }

        public override void Collect(RenderContext context)
        {
            if (PostEffects != null)
            {
                // Setup pixel formats for RenderStage
                var renderOutput = context.RenderOutputs.Peek();
                context.RenderOutputs.Push(new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : renderOutput.RenderTargetFormat0, PixelFormat.D24_UNorm_S8_UInt));
            }

            try
            {
                context.RenderSystem.Views.Add(MainRenderView);

                MainRenderView.SceneInstance = context.SceneInstance;
                var camera = MainRenderView.SceneInstance.Select(x => x.Get<CameraComponent>()).First(x => x != null);
                UpdateCameraToRenderView(context, MainRenderView, camera);

                UnitRenderer?.Collect(context, MainRenderView);
            }
            finally
            {
                if (PostEffects != null)
                {
                    context.RenderOutputs.Pop();
                }
            }
        }

        public override void Draw(RenderDrawContext context)
        {
            var viewport = context.CommandList.Viewport;
            var playerWidth = (int)viewport.Width;
            var playerHeight = (int)viewport.Height;

            var currentRenderTarget = context.CommandList.RenderTarget;
            var currentDepthStencil = context.CommandList.DepthStencilBuffer;
            context.PushRenderTargets();

            // Allocate render targets
            var renderTarget = PostEffects != null ? context.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(playerWidth, playerHeight, 1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget)) : currentRenderTarget;

            context.CommandList.Clear(renderTarget, Color.Green);
            context.CommandList.Clear(currentDepthStencil, DepthStencilClearOptions.DepthBuffer);
            context.CommandList.SetRenderTargetAndViewport(currentDepthStencil, renderTarget);
            UnitRenderer?.Draw(context, MainRenderView);

            // Run post effects
            if (PostEffects != null)
            {
                var camera = MainRenderView.SceneInstance.Select(x => x.Get<CameraComponent>()).First(x => x != null);
                PostEffects.Draw(context, renderTarget, currentDepthStencil, currentRenderTarget, camera);
            }

            context.PopRenderTargets();

            // Release render targets
            if (PostEffects != null)
                context.GraphicsContext.Allocator.ReleaseReference(renderTarget);
        }

        private static void UpdateCameraToRenderView(RenderContext context, RenderView renderView, CameraComponent camera)
        {
            //// Copy scene camera renderer data
            //renderView.CullingMask = sceneCameraRenderer.CullingMask;
            //renderView.CullingMode = sceneCameraRenderer.CullingMode;
            //renderView.ViewSize = new Vector2(sceneCameraRenderer.ComputedViewport.Width, sceneCameraRenderer.ComputedViewport.Height);

            // TODO: Multiple viewports?
            var currentViewport = context.ViewportStates.Peek().Viewport0;
            renderView.ViewSize = new Vector2(currentViewport.Width, currentViewport.Height);

            if (camera != null)
            {
                // Setup viewport size
                var aspectRatio = currentViewport.AspectRatio;

                // Update the aspect ratio
                if (camera.UseCustomAspectRatio)
                {
                    aspectRatio = camera.AspectRatio;
                }

                // If the aspect ratio is calculated automatically from the current viewport, update matrices here
                camera.Update(aspectRatio);

                // Copy camera data
                renderView.View = camera.ViewMatrix;
                renderView.Projection = camera.ProjectionMatrix;
                renderView.NearClipPlane = camera.NearClipPlane;
                renderView.FarClipPlane = camera.FarClipPlane;
                renderView.Frustum = camera.Frustum;

                Matrix.Multiply(ref renderView.View, ref renderView.Projection, out renderView.ViewProjection);
            }
        }
    }

    public partial class ForwardCompositor : GraphicsCompositorViewPart, IGraphicsCompositorSharedPart
    {
        public RenderStage MainRenderStage;

        public RenderStage ShadowMapRenderStage;

        public bool Shadows;

        public override void Collect(RenderContext context, RenderView mainRenderView)
        {
            var renderSystem = context.RenderSystem;

            // Fill RenderStage formats
            MainRenderStage.Output = context.RenderOutputs.Peek();
            if (ShadowMapRenderStage != null)
                ShadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);

            // Mark this view as requiring shadows
            // TODO: Remove LINQ + init phase?
            var shadowMapRenderer = renderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;
            shadowMapRenderer?.RenderViewsWithShadows.Add(mainRenderView);

            mainRenderView.RenderStages.Add(MainRenderStage);
        }

        public override void Draw(RenderDrawContext context, RenderView mainRenderView)
        {
            var renderSystem = context.RenderContext.RenderSystem;

            // Render Shadow maps
            // TODO: Remove LINQ + init phase?
            var shadowMapRenderer = renderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;
            if (Shadows && ShadowMapRenderStage != null && shadowMapRenderer != null)
            {
                // Clear atlases
                shadowMapRenderer.PrepareAtlasAsRenderTargets(context.CommandList);

                context.PushRenderTargets();

                // Draw all shadow views generated for the current view
                foreach (var renderView in renderSystem.Views)
                {
                    var shadowmapRenderView = renderView as ShadowMapRenderView;
                    if (shadowmapRenderView != null && shadowmapRenderView.RenderView == mainRenderView)
                    {
                        var shadowMapRectangle = shadowmapRenderView.Rectangle;
                        shadowmapRenderView.ShadowMapTexture.Atlas.RenderFrame.Activate(context);
                        shadowmapRenderView.ShadowMapTexture.Atlas.MarkClearNeeded();
                        context.CommandList.SetViewport(new Viewport(shadowMapRectangle.X, shadowMapRectangle.Y, shadowMapRectangle.Width, shadowMapRectangle.Height));

                        renderSystem.Draw(context, shadowmapRenderView, ShadowMapRenderStage);
                    }
                }

                context.PopRenderTargets();

                shadowMapRenderer.PrepareAtlasAsShaderResourceViews(context.CommandList);
            }

            renderSystem.Draw(context, mainRenderView, MainRenderStage);
        }
    }
}