// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    public interface ITopSceneRenderer : ISceneRenderer
    {
        RenderView MainRenderView { get; }

        ISceneRenderer Child { get; }
    }

    public interface IGizmoCompositor
    {
        ITopSceneRenderer TopLevel { get; }

        List<ISceneRenderer> PreGizmoCompositors { get; }

        List<ISceneRenderer> PostGizmoCompositors { get; }
    }

    public partial class EditorTopLevelCompositor : SceneRendererBase, ISharedRenderer, IGizmoCompositor
    {
        public ITopSceneRenderer TopLevel { get; set; }

        public List<ISceneRenderer> PreGizmoCompositors { get; } = new List<ISceneRenderer>();

        public List<ISceneRenderer> PostGizmoCompositors { get; } = new List<ISceneRenderer>();

        protected override void CollectCore(RenderContext context)
        {
            context.RenderView = TopLevel.MainRenderView;

            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Collect(context);

            TopLevel.Collect(context);

            foreach (var gizmoCompositor in PostGizmoCompositors)
                gizmoCompositor.Collect(context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            context.RenderContext.RenderView = TopLevel.MainRenderView;

            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Draw(context);

            TopLevel.Draw(context);

            foreach (var gizmoCompositor in PostGizmoCompositors)
                gizmoCompositor.Draw(context);
        }
    }

    // Note: Kept in a single file for easy iteration until the graphics compositor refactor is finished
    public partial class GizmoTopLevelCompositor : SceneRendererBase, ISharedRenderer
    {
        public ISceneRenderer UnitRenderer { get; set; }

        [DataMemberIgnore]
        public RenderView MainRenderView { get; } = new RenderView();

        protected override void CollectCore(RenderContext context)
        {
            context.RenderSystem.Views.Add(MainRenderView);
            context.RenderView = MainRenderView;

            MainRenderView.SceneInstance = context.SceneInstance;
            var camera = context.GetCurrentCamera();
            CameraViewCompositor.UpdateCameraToRenderView(context, MainRenderView, camera);

            UnitRenderer?.Collect(context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            context.RenderContext.RenderView = MainRenderView;

            UnitRenderer?.Draw(context);
        }
    }

    /// <summary>
    /// Defines and sets a <see cref="RenderView"/> and set it up using <see cref="Camera"/> or current context camera.
    /// </summary>
    /// <remarks>
    /// Since it sets a view, it is usually not shareable for multiple rendering.
    /// </remarks>
    public partial class CameraViewCompositor : SceneRendererBase, ITopSceneRenderer
    {
        [DataMemberIgnore]
        public RenderView MainRenderView { get; } = new RenderView();

        /// <summary>
        /// Overrides context camera (if not null).
        /// </summary>
        public SceneCameraSlotIndex Camera { get; set; } = new SceneCameraSlotIndex(0);

        public ISceneRenderer Child { get; set; }

        protected override void CollectCore(RenderContext renderContext)
        {
            base.CollectCore(renderContext);

            renderContext.RenderSystem.Views.Add(MainRenderView);

            MainRenderView.SceneInstance = renderContext.SceneInstance;
            var camera = renderContext.GetCameraFromSlot(Camera);
            UpdateCameraToRenderView(renderContext, MainRenderView, camera);

            var oldRenderView = renderContext.RenderView;
            renderContext.RenderView = MainRenderView;

            using (renderContext.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                Child?.Collect(renderContext);
            }

            renderContext.RenderView = oldRenderView;
        }

        protected override void DrawCore(RenderDrawContext renderContext)
        {
            var oldRenderView = renderContext.RenderContext.RenderView;
            renderContext.RenderContext.RenderView = MainRenderView;

            var camera = renderContext.RenderContext.GetCameraFromSlot(Camera);
            using (renderContext.RenderContext.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                Child?.Draw(renderContext);
            }

            renderContext.RenderContext.RenderView = oldRenderView;
        }

        internal static void UpdateCameraToRenderView(RenderContext context, RenderView renderView, CameraComponent camera)
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

    // Note: Kept in a single file for easy iteration until the graphics compositor refactor is finished
    public partial class TopLevelCompositor : SceneRendererBase, ISharedRenderer
    {
        public ISceneRenderer UnitRenderer { get; set; }

        public PostProcessingEffects PostEffects { get; set; }

        public Color4 ClearColor { get; set; } = Color.Green;

        protected override void CollectCore(RenderContext context)
        {
            if (PostEffects != null)
            {
                // Setup pixel formats for RenderStage
                var renderOutput = context.RenderOutputs.Peek();
                context.RenderOutputs.Push(new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : renderOutput.RenderTargetFormat0, PixelFormat.D24_UNorm_S8_UInt));
            }

            try
            {
                UnitRenderer?.Collect(context);
            }
            finally
            {
                if (PostEffects != null)
                {
                    context.RenderOutputs.Pop();
                }
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var viewport = context.CommandList.Viewport;
            var playerWidth = (int)viewport.Width;
            var playerHeight = (int)viewport.Height;

            var currentRenderTarget = context.CommandList.RenderTarget;
            var currentDepthStencil = context.CommandList.DepthStencilBuffer;
            context.PushRenderTargets();

            // Allocate render targets
            var renderTarget = PostEffects != null ? context.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(playerWidth, playerHeight, 1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget)) : currentRenderTarget;

            context.CommandList.Clear(renderTarget, ClearColor.ToColorSpace(context.GraphicsDevice.ColorSpace));
            context.CommandList.Clear(currentDepthStencil, DepthStencilClearOptions.DepthBuffer);
            context.CommandList.SetRenderTargetAndViewport(currentDepthStencil, renderTarget);
            UnitRenderer?.Draw(context);

            // Run post effects
            if (PostEffects != null)
            {
                PostEffects.Draw(context, renderTarget, currentDepthStencil, currentRenderTarget);
            }

            context.PopRenderTargets();

            // Release render targets
            if (PostEffects != null)
                context.GraphicsContext.Allocator.ReleaseReference(renderTarget);
        }
    }

    public partial class CompositeSceneRenderer : SceneRendererBase, IEnumerable<ISceneRenderer>
    {
        public List<ISceneRenderer> Children { get; } = new List<ISceneRenderer>();

        protected override void CollectCore(RenderContext renderContext)
        {
            base.CollectCore(renderContext);

            foreach (var child in Children)
                child.Collect(renderContext);
        }

        protected override void DrawCore(RenderDrawContext renderContext)
        {
            foreach (var child in Children)
                child.Draw(renderContext);
        }

        public void Add(ISceneRenderer child)
        {
            Children.Add(child);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ISceneRenderer> GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}