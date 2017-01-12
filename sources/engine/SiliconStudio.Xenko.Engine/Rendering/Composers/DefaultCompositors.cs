// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    public interface ITopSceneRenderer : ISceneRenderer
    {
        RenderView MainRenderView { get; }
    }

    public interface IGizmoCompositor
    {
        ISceneRenderer Child { get; }

        List<ISceneRenderer> PreGizmoCompositors { get; }

        List<ISceneRenderer> PostGizmoCompositors { get; }
    }

    public partial class EditorTopLevelCompositor : SceneCameraRenderer, ISharedRenderer, IGizmoCompositor
    {
        public List<ISceneRenderer> PreGizmoCompositors { get; } = new List<ISceneRenderer>();

        public List<ISceneRenderer> PostGizmoCompositors { get; } = new List<ISceneRenderer>();

        protected override void CollectInner(RenderContext context)
        {
            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Collect(context);

            base.CollectInner(context);

            foreach (var gizmoCompositor in PostGizmoCompositors)
                gizmoCompositor.Collect(context);
        }

        protected override void DrawInner(RenderDrawContext context)
        {
            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Draw(context);

            base.DrawInner(context);

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
            SceneCameraRenderer.UpdateCameraToRenderView(context, MainRenderView, camera);

            UnitRenderer?.Collect(context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            context.RenderContext.RenderView = MainRenderView;

            UnitRenderer?.Draw(context);
        }
    }

    // Note: Kept in a single file for easy iteration until the graphics compositor refactor is finished
    public partial class SingleViewPostProcessed : SceneRendererBase, ISharedRenderer
    {
        public ISceneRenderer UnitRenderer { get; set; }

        public PostProcessingEffects PostEffects { get; set; }

        public ClearRenderer Clear { get; } = new ClearRenderer();

        protected override void CollectCore(RenderContext context)
        {
            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                if (PostEffects != null)
                {
                    context.RenderOutput = new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : context.RenderOutput.RenderTargetFormat0, PixelFormat.D24_UNorm_S8_UInt);
                }

                UnitRenderer?.Collect(context);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var viewport = context.CommandList.Viewport;

            var currentRenderTarget = context.CommandList.RenderTarget;
            var currentDepthStencil = context.CommandList.DepthStencilBuffer;
            context.PushRenderTargets();

            // Allocate render targets
            var renderTarget = PostEffects != null ? context.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D((int)viewport.Width, (int)viewport.Height, 1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget)) : currentRenderTarget;

            Clear?.Draw(context);
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

    public partial class SceneRendererCollection : SceneRendererBase, IEnumerable<ISceneRenderer>
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