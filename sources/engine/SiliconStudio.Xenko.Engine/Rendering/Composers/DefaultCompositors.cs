// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public interface IGraphicsCompositorTopPart : IGraphicsCompositorPart
    {
        RenderView MainRenderView { get; }

        IGraphicsCompositorPart Child { get; }
    }

    public interface IGizmoCompositor
    {
        IGraphicsCompositorTopPart TopLevel { get; }

        List<IGraphicsCompositorPart> PreGizmoCompositors { get; }

        List<IGraphicsCompositorPart> PostGizmoCompositors { get; }
    }

    public partial class EditorTopLevelCompositor : GraphicsCompositorPart, IGraphicsCompositorSharedPart, IGizmoCompositor
    {
        public IGraphicsCompositorTopPart TopLevel { get; set; }

        public List<IGraphicsCompositorPart> PreGizmoCompositors { get; } = new List<IGraphicsCompositorPart>();

        public List<IGraphicsCompositorPart> PostGizmoCompositors { get; } = new List<IGraphicsCompositorPart>();

        public override void Collect(RenderContext context)
        {
            context.RenderView = TopLevel.MainRenderView;

            foreach (var gizmoCompositor in PreGizmoCompositors)
                gizmoCompositor.Collect(context);

            TopLevel.Collect(context);

            foreach (var gizmoCompositor in PostGizmoCompositors)
                gizmoCompositor.Collect(context);
        }

        public override void Draw(RenderDrawContext context)
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
    public partial class GizmoTopLevelCompositor : GraphicsCompositorPart, IGraphicsCompositorSharedPart
    {
        public IGraphicsCompositorPart UnitRenderer { get; set; }

        [DataMemberIgnore]
        public RenderView MainRenderView { get; } = new RenderView();

        public override void Collect(RenderContext context)
        {
            context.RenderSystem.Views.Add(MainRenderView);
            context.RenderView = MainRenderView;

            MainRenderView.SceneInstance = context.SceneInstance;
            var camera = context.GetCurrentCamera();
            CameraViewCompositor.UpdateCameraToRenderView(context, MainRenderView, camera);

            UnitRenderer?.Collect(context);
        }

        public override void Draw(RenderDrawContext context)
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
    public partial class CameraViewCompositor : GraphicsCompositorPart, IGraphicsCompositorTopPart
    {
        [DataMemberIgnore]
        public RenderView MainRenderView { get; } = new RenderView();

        /// <summary>
        /// Overrides context camera (if not null).
        /// </summary>
        [DataMemberIgnore]
        public CameraComponent Camera { get; set; }

        public IGraphicsCompositorPart Child { get; set; }

        public override void Collect(RenderContext renderContext)
        {
            base.Collect(renderContext);

            renderContext.RenderSystem.Views.Add(MainRenderView);

            MainRenderView.SceneInstance = renderContext.SceneInstance;
            var camera = Camera ?? renderContext.GetCurrentCamera();
            UpdateCameraToRenderView(renderContext, MainRenderView, camera);

            var oldRenderView = renderContext.RenderView;
            renderContext.RenderView = MainRenderView;

            using (renderContext.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                Child?.Collect(renderContext);
            }

            renderContext.RenderView = oldRenderView;
        }

        public override void Draw(RenderDrawContext renderContext)
        {
            base.Draw(renderContext);

            var oldRenderView = renderContext.RenderContext.RenderView;
            renderContext.RenderContext.RenderView = MainRenderView;

            var camera = Camera ?? renderContext.RenderContext.GetCurrentCamera();
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
    public partial class TopLevelCompositor : GraphicsCompositorPart, IGraphicsCompositorSharedPart
    {
        public IGraphicsCompositorPart UnitRenderer { get; set; }

        public PostProcessingEffects PostEffects { get; set; }

        public Color4 ClearColor { get; set; } = Color.Green;

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

    /// <summary>
    /// A renderer to clear a render frame.
    /// </summary>
    [Display("Clear RenderFrame")]
    public sealed class ClearCompositorPart : GraphicsCompositorPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClearRenderFrameRenderer"/> class.
        /// </summary>
        public ClearCompositorPart()
        {
            ClearFlags = ClearRenderFrameFlags.ColorAndDepth;
            Color = Core.Mathematics.Color.CornflowerBlue;
            Depth = 1.0f;
            Stencil = 0;
            ColorSpace = ColorSpace.Gamma;
        }

        /// <summary>
        /// Gets or sets the clear flags.
        /// </summary>
        /// <value>The clear flags.</value>
        /// <userdoc>Flag indicating which buffers to clear.</userdoc>
        [DataMember(10)]
        [DefaultValue(ClearRenderFrameFlags.ColorAndDepth)]
        [Display("Clear Flags")]
        public ClearRenderFrameFlags ClearFlags { get; set; }

        /// <summary>
        /// Gets or sets the clear color.
        /// </summary>
        /// <value>The clear color.</value>
        /// <userdoc>The color value to use when clearing the render targets</userdoc>
        [DataMember(20)]
        [Display("Color")]
        public Color4 Color { get; set; }

        /// <summary>
        /// Gets or sets the colorspace of the <see cref="Color"/> value. See remarks.
        /// </summary>
        /// <value>The clear color.</value>
        /// <userdoc>The colorspace of the color value. By default, the color is in gamma space and transformed automatically in linear space if the render target is either SRgb or HDR.</userdoc>
        [DataMember(25)]
        [DefaultValue(ColorSpace.Gamma)]
        [Display("Color Space")]
        public ColorSpace ColorSpace { get; set; }

        /// <summary>
        /// Gets or sets the depth value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The depth value used to clear the depth stencil buffer.
        /// </value>
        /// <userdoc>The depth value to use when clearing the depth buffer</userdoc>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        [Display("Depth Value")]
        public float Depth { get; set; }

        /// <summary>
        /// Gets or sets the stencil value used to clear the depth stencil buffer.
        /// </summary>
        /// <value>
        /// The stencil value used to clear the depth stencil buffer.
        /// </value>
        /// <userdoc>The stencil value to use when clearing the stencil buffer</userdoc>
        [DataMember(40)]
        [DefaultValue(0)]
        [Display("Stencil Value")]
        public byte Stencil { get; set; }

        public override void Draw(RenderDrawContext renderContext)
        {
            base.Draw(renderContext);

            var commandList = renderContext.CommandList;

            var depthStencil = commandList.DepthStencilBuffer;

            // clear the targets
            if (depthStencil != null && (ClearFlags == ClearRenderFrameFlags.ColorAndDepth || ClearFlags == ClearRenderFrameFlags.DepthOnly))
            {
                var clearOptions = DepthStencilClearOptions.DepthBuffer;
                if (depthStencil.HasStencil)
                    clearOptions |= DepthStencilClearOptions.Stencil;

                commandList.Clear(depthStencil, clearOptions, Depth, Stencil);
            }

            if (ClearFlags == ClearRenderFrameFlags.ColorAndDepth || ClearFlags == ClearRenderFrameFlags.ColorOnly)
            {
                for (var index = 0; index < commandList.RenderTargetCount; index++)
                {
                    var renderTarget = commandList.RenderTargets[index];
                    // If color is in GammeSpace and rendertarget is either SRgb or HDR, use a linear value to clear the buffer.
                    // TODO: We will need to move this color transform code to a shareable component
                    var color = Color.ToColorSpace(renderContext.GraphicsDevice.ColorSpace);
                    commandList.Clear(renderTarget, color);
                }
            }
        }
    }

    public partial class DelegateCompositorPart : GraphicsCompositorPart
    {
        private Action<RenderDrawContext> drawAction;

        public DelegateCompositorPart(Action<RenderDrawContext> drawAction)
        {
            this.drawAction = drawAction;
        }

        public override void Draw(RenderDrawContext renderContext)
        {
            base.Draw(renderContext);

            drawAction(renderContext);
        }
    }

    public partial class CompositeCompositorPart : GraphicsCompositorPart, IEnumerable<IGraphicsCompositorPart>
    {
        public List<IGraphicsCompositorPart> Children { get; } = new List<IGraphicsCompositorPart>();

        public override void Collect(RenderContext renderContext)
        {
            base.Collect(renderContext);

            foreach (var child in Children)
                child.Collect(renderContext);
        }

        public override void Draw(RenderDrawContext renderContext)
        {
            base.Draw(renderContext);

            foreach (var child in Children)
                child.Draw(renderContext);
        }

        public void Add(IGraphicsCompositorPart child)
        {
            Children.Add(child);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IGraphicsCompositorPart> GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }


    public partial class DirectCompositor : GraphicsCompositorPart, IGraphicsCompositorSharedPart
    {
        public RenderStage RenderStage;

        public override void Collect(RenderContext context)
        {
            // Fill RenderStage formats
            RenderStage.Output = context.RenderOutputs.Peek();

            context.RenderView.RenderStages.Add(RenderStage);
        }

        public override void Draw(RenderDrawContext context)
        {
            context.RenderContext.RenderSystem.Draw(context, context.RenderContext.RenderView, RenderStage);
        }
    }

    public partial class ForwardCompositor : GraphicsCompositorPart, IGraphicsCompositorSharedPart
    {
        public RenderStage MainRenderStage;
        public RenderStage TransparentRenderStage;

        public RenderStage ShadowMapRenderStage;

        public bool Shadows;

        public override void Collect(RenderContext context)
        {
            var renderSystem = context.RenderSystem;

            // Mark this view as requiring shadows
            // TODO: Remove LINQ + init phase?
            var shadowMapRenderer = renderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;
            shadowMapRenderer?.RenderViewsWithShadows.Add(context.RenderView);

            // Fill RenderStage formats and register render stages to main view
            if (MainRenderStage != null)
            {
                context.RenderView.RenderStages.Add(MainRenderStage);
                MainRenderStage.Output = context.RenderOutputs.Peek();
            }
            if (TransparentRenderStage != null)
            {
                context.RenderView.RenderStages.Add(TransparentRenderStage);
                TransparentRenderStage.Output = context.RenderOutputs.Peek();
            }

            if (ShadowMapRenderStage != null)
                ShadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);
        }

        public override void Draw(RenderDrawContext context)
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
                    if (shadowmapRenderView != null && shadowmapRenderView.RenderView == context.RenderContext.RenderView)
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

            if (MainRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, MainRenderStage);
            if (TransparentRenderStage != null)
                renderSystem.Draw(context, context.RenderContext.RenderView, TransparentRenderStage);
        }
    }
}