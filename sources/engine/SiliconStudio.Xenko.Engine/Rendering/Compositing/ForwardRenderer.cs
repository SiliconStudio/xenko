using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.VirtualReality;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public class DefaultRenderTargets : IColorTarget, INormalTarget, IVelocityTarget, IMultipleRenderViews
    {
        private readonly Texture[] allTargets = new Texture[3];

        public Texture Color { get; set; }

        public Texture Normal { get; set; }

        public Texture Velocity { get; set; }

        public Texture[] AllTargets
        {
            get
            {
                //color
                allTargets[0] = Color;

                //normals
                if (Normal != null)
                {
                    allTargets[1] = Normal;
                }

                //velocity
                if (Normal == null)
                {
                    allTargets[1] = Velocity;
                }
                else
                {
                    allTargets[2] = Velocity;
                }

                return allTargets;
            }
        }

        public int NumberOfTargets
        {
            get
            {
                var n = 0;
                if (Color != null)
                    n++;
                if (Normal != null)
                    n++;
                if (Velocity != null)
                    n++;
                return n;
            }
        }

        public int Count { get; set; }

        public int Index { get; set; }
    }

    /// <summary>
    /// Renders your game. It should use current <see cref="RenderContext.RenderView"/> and <see cref="CameraComponentRendererExtensions.GetCurrentCamera"/>.
    /// </summary>
    public partial class ForwardRenderer : SceneRendererBase, ISharedRenderer
    {
        private IShadowMapRenderer shadowMapRenderer;
        private Texture depthStencilROCached;
        private MSAALevel actualMSAALevel = MSAALevel.None;

        protected IRenderTarget ViewTargetsComposition;
        protected IRenderTarget ViewTargetsCompositionNoMSAA;
        protected Texture ViewOutputTarget;
        protected Texture ViewDepthStencil;
        protected Texture ViewDepthStencilNoMSAA;
        private VRDeviceSystem vrSystem;

        public ClearRenderer Clear { get; set; } = new ClearRenderer();
        
        /// <summary>
        /// Enable Light Probe.
        /// </summary>
        public bool LightProbes { get; set; } = true;

        /// <summary>
        /// The main render stage for opaque geometry.
        /// </summary>
        public RenderStage OpaqueRenderStage { get; set; }

        /// <summary>
        /// The transparent render stage for transparent geometry.
        /// </summary>
        public RenderStage TransparentRenderStage { get; set; }

        /// <summary>
        /// The shadow map render stages for shadow casters. No shadow rendering will happen if null.
        /// </summary>
        public List<RenderStage> ShadowMapRenderStages { get; } = new List<RenderStage>();

        /// <summary>
        /// The G-Buffer render stage to render depth buffer and possibly some other extra info to buffers (i.e. normals)
        /// </summary>
        public RenderStage GBufferRenderStage { get; set; }

        /// <summary>
        /// The post effects renderer.
        /// </summary>
        public IPostProcessingEffects PostEffects { get; set; }

        /// <summary>
        /// Light shafts effect
        /// </summary>
        public LightShafts LightShafts { get; set; }

        /// <summary>
        /// Virtual Reality related settings
        /// </summary>
        public VRRendererSettings VRSettings { get; set; } = new VRRendererSettings();

        /// <summary>
        /// The level of multi-sampling
        /// </summary>
        public MSAALevel MSAALevel { get; set; } = MSAALevel.None;

        /// <summary>
        /// If true, depth buffer generated during <see cref="OpaqueRenderStage"/> will be available as a shader resource named DepthBase.DepthStencil during <see cref="TransparentRenderStage"/>.
        /// </summary>
        /// <remarks>
        /// This is needed by some effects such as particles soft edges.
        /// 
        /// On recent platforms that can bind depth buffer as read-only (<see cref="GraphicsDeviceFeatures.HasDepthAsReadOnlyRT"/>), depth buffer will be used as is. Otherwise, a copy will be generated.
        /// </remarks>
        [DefaultValue(true)]
        public bool BindDepthAsResourceDuringTransparentRendering { get; set; } = true;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            ViewTargetsComposition = new DefaultRenderTargets();
            ViewTargetsCompositionNoMSAA = new DefaultRenderTargets();

            shadowMapRenderer =
                Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;

            if (MSAALevel != MSAALevel.None)
            {
                actualMSAALevel = (MSAALevel)Math.Min((int)MSAALevel, (int)GraphicsDevice.Features[PixelFormat.R16G16B16A16_Float].MSAALevelMax);
                actualMSAALevel = (MSAALevel)Math.Min((int)actualMSAALevel, (int)GraphicsDevice.Features[PixelFormat.D24_UNorm_S8_UInt].MSAALevelMax);
            }

            var camera = Context.GetCurrentCamera();

            vrSystem = (VRDeviceSystem)Services.GetService(typeof(VRDeviceSystem));
            if (vrSystem != null)
            {
                if (VRSettings.Enabled)
                {
                    vrSystem.PreferredApis = VRSettings.RequiredApis.ToArray();
                    vrSystem.RequireMirror = true;
                    vrSystem.MirrorWidth = GraphicsDevice.Presenter.BackBuffer.Width;
                    vrSystem.MirrorHeight = GraphicsDevice.Presenter.BackBuffer.Height;
                    vrSystem.ResolutionScale = VRSettings.ResolutionScale;

                    vrSystem.Enabled = true; //careful this will trigger the whole chain of initialization!
                    vrSystem.Visible = true;
                    
                    VRSettings.VRDevice = vrSystem.Device;

                    vrSystem.PreviousUseCustomProjectionMatrix = camera.UseCustomProjectionMatrix;
                    vrSystem.PreviousUseCustomViewMatrix = camera.UseCustomViewMatrix;
                    vrSystem.PreviousCameraProjection = camera.ProjectionMatrix;
                }
                else
                {
                    vrSystem.Enabled = false;
                    vrSystem.Visible = false;

                    VRSettings.VRDevice = null;

                    if (vrSystem.Device != null) //we had a device before so we know we need to restore the camera
                    {
                        camera.UseCustomViewMatrix = vrSystem.PreviousUseCustomViewMatrix;
                        camera.UseCustomProjectionMatrix = vrSystem.PreviousUseCustomProjectionMatrix;
                        camera.ProjectionMatrix = vrSystem.PreviousCameraProjection;
                    }
                }
            }
        }

        protected virtual void CollectView(RenderContext context)
        {
            // Fill RenderStage formats and register render stages to main view
            if (OpaqueRenderStage != null)
            {
                context.RenderView.RenderStages.Add(OpaqueRenderStage);
                OpaqueRenderStage.Output = context.RenderOutput;
            }
            if (TransparentRenderStage != null)
            {
                context.RenderView.RenderStages.Add(TransparentRenderStage);
                TransparentRenderStage.Output = context.RenderOutput;
            }

            if (GBufferRenderStage != null && LightProbes)
            {
                context.RenderView.RenderStages.Add(GBufferRenderStage);
                GBufferRenderStage.Output = new RenderOutputDescription(PixelFormat.None, context.RenderOutput.DepthStencilFormat);
            }
        }

        protected override unsafe void CollectCore(RenderContext context)
        {
            var camera = context.GetCurrentCamera();

            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                // Mark this view as requiring shadows
                shadowMapRenderer?.RenderViewsWithShadows.Add(context.RenderView);

                context.RenderOutput = new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : context.RenderOutput.RenderTargetFormat0, PixelFormat.D24_UNorm_S8_UInt);

                if (VRSettings.Enabled && VRSettings.VRDevice != null)
                {
                    Vector3 cameraPos, cameraScale;
                    Matrix cameraRot;

                    if (!vrSystem.PreviousUseCustomViewMatrix)
                    {
                        camera.Entity.Transform.WorldMatrix.Decompose(out cameraScale, out cameraRot, out cameraPos);
                    }
                    else
                    {
                        camera.ViewMatrix.Decompose(out cameraScale, out cameraRot, out cameraPos);
                        cameraRot.Transpose();
                        Vector3.Negate(ref cameraPos, out cameraPos);
                        Vector3.TransformCoordinate(ref cameraPos, ref cameraRot, out cameraPos);
                    }

                    if (VRSettings.IgnoreCameraRotation)
                    {
                        cameraRot = Matrix.Identity;
                    }

                    // Compute both view and projection matrices
                    Matrix* viewMatrices = stackalloc Matrix[2];
                    Matrix* projectionMatrices = stackalloc Matrix[2];
                    for (var i = 0; i < 2; ++i)
                        VRSettings.VRDevice.ReadEyeParameters(i == 0 ? Eyes.Left : Eyes.Right, camera.NearClipPlane, camera.FarClipPlane, ref cameraPos, ref cameraRot, out viewMatrices[i], out projectionMatrices[i]);

                    // Compute a view matrix and projection matrix that cover both eyes for shadow map and culling
                    var commonView = context.RenderView;
                    commonView.View = viewMatrices[0];
                    // We assume view matrices are similar except for a translation; we can take the average to have the "center eye" position
                    commonView.View.M41 = commonView.View.M41 * 0.5f + viewMatrices[1].M41 * 0.5f;
                    commonView.View.M42 = commonView.View.M42 * 0.5f + viewMatrices[1].M42 * 0.5f;
                    commonView.View.M43 = commonView.View.M43 * 0.5f + viewMatrices[1].M43 * 0.5f;

                    // Also need to move it backward little bit
                    // http://computergraphics.stackexchange.com/questions/1736/vr-and-frustum-culling

                    // Projection: Need to extend size to cover equivalent of both eyes
                    // So we cancel the left/right off-center and add it to the width to compensate
                    commonView.Projection = projectionMatrices[0];
                    // Compute left and right
                    var left0 = commonView.NearClipPlane * (projectionMatrices[0].M31 - 1.0f) / projectionMatrices[0].M11;
                    var right1 = commonView.NearClipPlane * (projectionMatrices[1].M31 + 1.0f) / projectionMatrices[1].M11;
                    commonView.Projection.M11 = 2.0f * commonView.NearClipPlane / (right1 - left0);
                    commonView.Projection.M31 = (right1 + left0) / (right1 - left0);

                    // Collect now, and use result for both eyes
                    CollectView(context);
                    context.VisibilityGroup.TryCollect(commonView);

                    for (var i = 0; i < 2; i++)
                    {
                        using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                        using (context.SaveViewportAndRestore())
                        {
                            context.RenderSystem.Views.Add(context.RenderView);
                            context.RenderView.SceneInstance = commonView.SceneInstance;
                            context.RenderView.LightingView = commonView;
                            context.ViewportState.Viewport0 = new Viewport(0, 0, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2.0f, VRSettings.VRDevice.ActualRenderFrameSize.Height);

                            //change camera params for eye
                            camera.ViewMatrix = viewMatrices[i];
                            camera.ProjectionMatrix = projectionMatrices[i];
                            camera.UseCustomProjectionMatrix = true;
                            camera.UseCustomViewMatrix = true;
                            camera.Update();

                            //write params to view
                            SceneCameraRenderer.UpdateCameraToRenderView(context, context.RenderView, camera);

                            // Copy culling results
                            context.VisibilityGroup.Copy(commonView, context.RenderView);

                            CollectView(context);

                            LightShafts?.Collect(context);

                            PostEffects?.Collect(context);
                        }
                    }
                }
                else
                {
                    //write params to view
                    SceneCameraRenderer.UpdateCameraToRenderView(context, context.RenderView, camera);

                    CollectView(context);

                    LightShafts?.Collect(context);

                    PostEffects?.Collect(context);
                }

                // Set depth format for shadow map render stages
                // TODO: This format should be acquired from the ShadowMapRenderer instead of being fixed here
                foreach(var shadowMapRenderStage in ShadowMapRenderStages)
                    shadowMapRenderStage.Output = new RenderOutputDescription(PixelFormat.None, PixelFormat.D32_Float);
            }

            PostEffects?.Collect(context);
        }

        protected virtual void ResolveDepthMSAA(RenderDrawContext drawContext)
        {
            ViewDepthStencilNoMSAA = PushScopedResource(
                drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(ViewOutputTarget.Size.Width, ViewOutputTarget.Size.Height,
                1, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.ShaderResource | TextureFlags.DepthStencil)));

            drawContext.CommandList.CopyMultiSample(ViewDepthStencil, 0, ViewDepthStencilNoMSAA, 0, PixelFormat.R24_UNorm_X8_Typeless);

        }

        protected virtual void ResolveMSAA(RenderDrawContext drawContext)
        {
            var colorIn = ViewTargetsComposition as IColorTarget;
            var colorOut = ViewTargetsCompositionNoMSAA as IColorTarget;
            if (colorIn != null && colorOut != null)
            {
                colorOut.Color = PushScopedResource(
                    drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(ViewOutputTarget.Size.Width, ViewOutputTarget.Size.Height,
                        1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget)));

                drawContext.CommandList.CopyMultiSample(colorIn.Color, 0, colorOut.Color, 0);
            }

            var normalsIn = ViewTargetsComposition as INormalTarget;
            var normalsOut = ViewTargetsCompositionNoMSAA as INormalTarget;
            if (normalsIn != null && normalsOut != null && PostEffects.RequiresNormalBuffer)
            {
                normalsOut.Normal = PushScopedResource(
                    drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(ViewOutputTarget.Size.Width, ViewOutputTarget.Size.Height,
                        1, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget)));

                drawContext.CommandList.CopyMultiSample(normalsIn.Normal, 0, normalsOut.Normal, 0);
            }

            var velocityIn = ViewTargetsComposition as IVelocityTarget;
            var velocityOut = ViewTargetsCompositionNoMSAA as IVelocityTarget;
            if (velocityIn != null && velocityOut != null && PostEffects.RequiresVelocityBuffer)
            {
                velocityOut.Velocity = PushScopedResource(
                    drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(ViewOutputTarget.Size.Width, ViewOutputTarget.Size.Height,
                        1, PixelFormat.R16G16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget)));

                drawContext.CommandList.CopyMultiSample(velocityIn.Velocity, 0, velocityOut.Velocity, 0);
            }

            var viewsIn = ViewTargetsComposition as IMultipleRenderViews;
            var viewsOut = ViewTargetsCompositionNoMSAA as IMultipleRenderViews;
            if (viewsIn != null && viewsOut != null)
            {
                viewsOut.Count = viewsIn.Count;
                viewsOut.Index = viewsIn.Index;
            }
        }

        protected virtual void DrawView(RenderContext context, RenderDrawContext drawContext)
        {
            var renderSystem = context.RenderSystem;

            // Z Prepass
            var lightProbes = LightProbes && GBufferRenderStage != null;
            if (lightProbes)
            {
                // Note: Baking lightprobe before GBuffer prepass because we are updating some cbuffer parameters needed by Opaque pass that GBuffer pass might upload early
                PrepareLightprobeConstantBuffer(context);

                // TODO: Temporarily using ShadowMap shader
                drawContext.CommandList.BeginProfile(Color.Green, "GBuffer");

                using (drawContext.PushRenderTargetsAndRestore())
                {
                    drawContext.CommandList.Clear(drawContext.CommandList.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
                    drawContext.CommandList.SetRenderTarget(drawContext.CommandList.DepthStencilBuffer, null);

                    // Draw [main view | z-prepass stage]
                    renderSystem.Draw(drawContext, context.RenderView, GBufferRenderStage);
                }

                drawContext.CommandList.EndProfile();

                // Bake lightprobes against Z-buffer
                BakeLightProbes(context, drawContext);
            }

            using (drawContext.PushRenderTargetsAndRestore())
            {
                // Draw [main view | main stage]
                if (OpaqueRenderStage != null)
                {
                    renderSystem.Draw(drawContext, context.RenderView, OpaqueRenderStage);
                }

                // Draw [main view | transparent stage]
                if (TransparentRenderStage != null)
                {
                    // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        var depthStencilSRV = ResolveDepthAsSRV(drawContext);

                        renderSystem.Draw(drawContext, context.RenderView, TransparentRenderStage);

                        // Free the depth texture since we won't need it anymore
                        if (depthStencilSRV != null)
                        {
                            drawContext.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
                        }
                    }
                }


                if (PostEffects != null)
                {
                    //Make sure we run post effects without MSAA
                    var peInputTargets = ViewTargetsComposition;
                    var peInputDepth = ViewDepthStencil;

                    //Shafts if we have them, with MSAA if we have it
                    LightShafts?.Draw(drawContext, peInputTargets, peInputDepth, ViewOutputTarget);

                    //Remove MSAA
                    if (actualMSAALevel != MSAALevel.None)
                    {
                        ResolveMSAA(drawContext);
                        // If lightprobes (which need Z-Prepass) are enabled, depth is already resolved
                        //if (!lightProbes)
                        //    ResolveDepthMSAA(drawContext);
                        peInputTargets = ViewTargetsCompositionNoMSAA;
                        //peInputDepth = ViewDepthStencilNoMSAA;
                    }

                    // Run post effects
                    PostEffects.Draw(drawContext, peInputTargets, peInputDepth, ViewOutputTarget);
                }
                else
                {
                    if (actualMSAALevel != MSAALevel.None)
                    {
                        var color = ViewTargetsComposition as IColorTarget;
                        if (color != null)
                        {
                            drawContext.CommandList.CopyMultiSample(color.Color, 0, ViewOutputTarget, 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepares targets per frame, caching and handling MSAA etc.
        /// </summary>
        /// <param name="drawContext">The current draw context</param>
        /// <param name="renderTargetsSize"></param>
        protected virtual void PrepareRenderTargets(RenderDrawContext drawContext, Size2 renderTargetsSize)
        {
            var currentRenderTarget = drawContext.CommandList.RenderTarget;
            if (drawContext.CommandList.RenderTargetCount == 0)
                currentRenderTarget = null;
            var currentDepthStencil = drawContext.CommandList.DepthStencilBuffer;

            //Make sure we got a valid NOT MSAA OUTPUT Target
            if (currentRenderTarget == null)
            {
                currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                        TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
            }

            //MSAA, we definitely need new buffers
            if (actualMSAALevel != MSAALevel.None)
            {
                //Handle color render targets
                var color = ViewTargetsComposition as IColorTarget;
                if (color != null)
                {
                    color.Color = PostEffects != null
                    ? PushScopedResource(
                        drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R16G16B16A16_Float,
                            TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)))

                    : PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D( //msaa but no HDR, use RGB8 temp buffer
                        TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                            TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)));
                }

                //Handle Depth
                ViewDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil, 1, GraphicsResourceUsage.Default, actualMSAALevel)));

            }
            else
            {
                if (PostEffects == null) //NO Post-Effects
                {
                    //Handle color
                    var color = ViewTargetsComposition as IColorTarget;
                    if (color != null)
                    {
                        color.Color = currentRenderTarget;
                    }
                }
                else // WITH Post-Effects
                {
                    //Handle color
                    var color = ViewTargetsComposition as IColorTarget;
                    if (color != null)
                    {
                        color.Color = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                            TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R16G16B16A16_Float,
                                TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                    }
                }

                //Handle Depth
                if (currentDepthStencil == null)
                {
                    currentDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil)));
                }

                ViewDepthStencil = currentDepthStencil;
            }

            //Handle normals
            var normals = ViewTargetsComposition as INormalTarget;
            if (normals != null)
            {
                normals.Normal = PostEffects != null && PostEffects.RequiresNormalBuffer
                ? PushScopedResource(
                    drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R16G16B16A16_Float,
                        TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)))
                : null;
            }

            //Handle velocity
            var velocity = ViewTargetsComposition as IVelocityTarget;
            if (velocity != null)
            {
                velocity.Velocity = PostEffects != null && PostEffects.RequiresVelocityBuffer
                ? PushScopedResource(
                    drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R16G16_Float,
                        TextureFlags.ShaderResource | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default, actualMSAALevel)))
                : null;
            }

            ViewOutputTarget = currentRenderTarget;
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            var viewport = drawContext.CommandList.Viewport;

            using (drawContext.PushRenderTargetsAndRestore())
            {
                // Render Shadow maps
                shadowMapRenderer?.Draw(drawContext);

                if (VRSettings.Enabled && VRSettings.VRDevice != null)
                {
                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        //make sure we don't use any default targets!
                        drawContext.CommandList.SetRenderTargets(null, null);

                        PrepareRenderTargets(drawContext, new Size2(VRSettings.VRDevice.ActualRenderFrameSize.Width / 2, VRSettings.VRDevice.ActualRenderFrameSize.Height));

                        //also prepare the final VR target
                        var vrFullSurface = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                                                TextureDescription.New2D(VRSettings.VRDevice.ActualRenderFrameSize.Width, VRSettings.VRDevice.ActualRenderFrameSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                                                    TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                     
                        //draw per eye
                        using (context.SaveViewportAndRestore())
                        using (drawContext.PushRenderTargetsAndRestore())
                        {
                            drawContext.CommandList.SetViewport(new Viewport(0.0f, 0.0f, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2.0f, VRSettings.VRDevice.ActualRenderFrameSize.Height));
                            drawContext.CommandList.SetRenderTargets(ViewDepthStencil, ViewTargetsComposition.NumberOfTargets, ViewTargetsComposition.AllTargets);

                            var views = ViewTargetsComposition as IMultipleRenderViews;
                            if (views != null)
                                views.Count = 2;

                            for (var i = 0; i < 2; i++)
                            {
                                using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                                {
                                    // Clear render target and depth stencil
                                    Clear?.Draw(drawContext);

                                    if (views != null)
                                        views.Index = i;

                                    DrawView(context, drawContext);
                                    drawContext.CommandList.CopyRegion(ViewOutputTarget, 0, null, vrFullSurface, 0, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2 * i);
                                }
                            }

                            VRSettings.VRDevice.Commit(drawContext.CommandList, vrFullSurface);
                        }
                    }

                    //draw mirror to backbuffer
                    if (VRSettings.VRDevice.MirrorTexture.Size != drawContext.CommandList.RenderTarget.Size)
                    {
                        VRSettings.MirrorScaler.SetInput(0, VRSettings.VRDevice.MirrorTexture);
                        VRSettings.MirrorScaler.SetOutput(drawContext.CommandList.RenderTarget);
                        VRSettings.MirrorScaler.Draw(drawContext);
                    }
                    else
                    {
                        drawContext.CommandList.Copy(VRSettings.VRDevice.MirrorTexture, drawContext.CommandList.RenderTarget);
                    }                  
                }
                else
                {
                    PrepareRenderTargets(drawContext, new Size2((int)viewport.Width, (int)viewport.Height));

                    var views = ViewTargetsComposition as IMultipleRenderViews;
                    if (views != null)
                    {
                        views.Count = 1;
                        views.Index = 0;
                    }

                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        drawContext.CommandList.SetRenderTargetsAndViewport(ViewDepthStencil, ViewTargetsComposition.NumberOfTargets, ViewTargetsComposition.AllTargets);

                        // Clear render target and depth stencil
                        Clear?.Draw(drawContext);

                        DrawView(context, drawContext);
                    }
                }
            }
        }

        private Texture ResolveDepthAsSRV(RenderDrawContext context)
        {
            if (!BindDepthAsResourceDuringTransparentRendering)
                return null;

            var depthStencil = context.CommandList.DepthStencilBuffer;
            var depthStencilSRV = context.Resolver.ResolveDepthStencil(context.CommandList.DepthStencilBuffer);

            var renderView = context.RenderContext.RenderView;

            foreach (var renderFeature in context.RenderContext.RenderSystem.RenderFeatures)
            {
                if (!(renderFeature is RootEffectRenderFeature))
                    continue;

                var depthLogicalKey = ((RootEffectRenderFeature)renderFeature).CreateViewLogicalGroup("Depth");
                var viewFeature = renderView.Features[renderFeature.Index];

                // Copy ViewProjection to PerFrame cbuffer
                foreach (var viewLayout in viewFeature.Layouts)
                {
                    var resourceGroup = viewLayout.Entries[renderView.Index].Resources;

                    var depthLogicalGroup = viewLayout.GetLogicalGroup(depthLogicalKey);
                    if (depthLogicalGroup.Hash == ObjectId.Empty)
                        continue;

                    // Might want to use ProcessLogicalGroup if more than 1 Recource
                    resourceGroup.DescriptorSet.SetShaderResourceView(depthLogicalGroup.DescriptorSlotStart, depthStencilSRV);
                }
            }
            
            context.CommandList.SetRenderTargets(null, context.CommandList.RenderTargetCount, context.CommandList.RenderTargets);

            depthStencilROCached = context.Resolver.GetDepthStencilAsRenderTarget(depthStencil, depthStencilROCached);
            context.CommandList.SetRenderTargets(depthStencilROCached, context.CommandList.RenderTargetCount, context.CommandList.RenderTargets);

            return depthStencilSRV;
        }

        protected override void Destroy()
        {
            PostEffects?.Dispose();
        }
    }
}