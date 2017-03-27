using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.VirtualReality;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// Renders your game. It should use current <see cref="RenderContext.RenderView"/> and <see cref="CameraComponentRendererExtensions.GetCurrentCamera"/>.
    /// </summary>
    public partial class ForwardRenderer : SceneRendererBase, ISharedRenderer
    {
        // TODO: should we use GraphicsDeviceManager.PreferredBackBufferFormat?
        public const PixelFormat DepthBufferFormat = PixelFormat.D24_UNorm_S8_UInt;

        private IShadowMapRenderer shadowMapRenderer;
        private Texture depthStencilROCached;
        private MSAALevel actualMSAALevel = MSAALevel.None;

        public RenderTargetSetup TargetsComposition;
        protected RenderTargetSetup TargetsCompositionNoMSAA;
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
        /// MSAA Resolver is used to resolve multi-sampled render targets into normal render targets
        /// </summary>
        public MSAAResolver MSAAResolver { get; } = new MSAAResolver();

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

            TargetsComposition = new RenderTargetSetup();
            TargetsCompositionNoMSAA = new RenderTargetSetup();

            shadowMapRenderer =
                Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault()?.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault()?.ShadowMapRenderer;

            if (MSAALevel != MSAALevel.None)
            {
                actualMSAALevel = (MSAALevel)Math.Min((int)MSAALevel, (int)GraphicsDevice.Features[PixelFormat.R16G16B16A16_Float].MSAALevelMax);
                actualMSAALevel = (MSAALevel)Math.Min((int)actualMSAALevel, (int)GraphicsDevice.Features[DepthBufferFormat].MSAALevelMax);

                // Note: we cannot support MSAA on DX10 now
                if(GraphicsDevice.Features.HasMSAADepthAsSRV == false)
                    actualMSAALevel = MSAALevel.None;
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
                CollectOpaqueStageRenderTargetComposition(context);
                context.RenderView.RenderStages.Add(OpaqueRenderStage);
                OpaqueRenderStage.Output = context.RenderOutput;
                OpaqueRenderStage.RenderTargetComposition = TargetsComposition;
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

            if (context.RenderView == null)
                throw new NullReferenceException(nameof(context.RenderView) + " is null. Please make sure you have your camera correctly set.");

            // Setup pixel formats for RenderStage
            using (context.SaveRenderOutputAndRestore())
            {
                // Mark this view as requiring shadows
                shadowMapRenderer?.RenderViewsWithShadows.Add(context.RenderView);

                context.RenderOutput = new RenderOutputDescription(PostEffects != null ? PixelFormat.R16G16B16A16_Float : context.RenderOutput.RenderTargetFormat0, DepthBufferFormat);

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

                    // if the VRDevice disagreed with the near and far plane, we must re-discover them and follow:
                    var near = projectionMatrices[0].M43 / projectionMatrices[0].M33;
                    var far = near * (-projectionMatrices[0].M33 / (-projectionMatrices[0].M33 - 1));
                    if (Math.Abs(near - camera.NearClipPlane) > 1e-8f)
                        camera.NearClipPlane = near;
                    if (Math.Abs(near - camera.FarClipPlane) > 1e-8f)
                        camera.FarClipPlane = far;

                    // Compute a view matrix and projection matrix that cover both eyes for shadow map and culling
                    ComputeCommonViewMatrices(context, viewMatrices, projectionMatrices);
                    var commonView = context.RenderView;

                    // Notify lighting system this view only purpose is for shared lighting, it is not being drawn directly.
                    commonView.Flags |= RenderViewFlags.NotDrawn;

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

        protected static PixelFormat ComputeNonMSAADepthFormat(PixelFormat format)
        {
            PixelFormat result;

            switch (format)
            {
                case PixelFormat.R16_Float:
                case PixelFormat.R16_Typeless:
                case PixelFormat.D16_UNorm:
                    result = PixelFormat.R16_Float;
                    break;
                case PixelFormat.R32_Float:
                case PixelFormat.R32_Typeless:
                case PixelFormat.D32_Float:
                    result = PixelFormat.R32_Float;
                    break;

                // Note: for those formats we lose stencil buffer information during MSAA -> non-MSAA conversion
                case PixelFormat.R24G8_Typeless:
                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.R24_UNorm_X8_Typeless:
                    result = PixelFormat.R32_Float;
                    break;
                case PixelFormat.R32G8X24_Typeless:
                case PixelFormat.D32_Float_S8X24_UInt:
                case PixelFormat.R32_Float_X8X24_Typeless:
                    result = PixelFormat.R32_Float;
                    break;

                default:
                    throw new NotSupportedException($"Unsupported depth format [{format}]");
            }

            return result;
        }

        protected virtual void ResolveDepthMSAA(RenderDrawContext drawContext)
        {
            if (ViewDepthStencil.MultiSampleLevel == MSAALevel.None)
            {
                ViewDepthStencilNoMSAA = ViewDepthStencil;
                return;
            }

            ViewDepthStencilNoMSAA = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(
                ViewDepthStencil.ViewWidth, ViewDepthStencil.ViewHeight, 1, ComputeNonMSAADepthFormat(ViewDepthStencil.Format), TextureFlags.RenderTarget | TextureFlags.ShaderResource)));

            ResolveMSAA(drawContext, ViewDepthStencil, ViewDepthStencilNoMSAA, 1);
        }

        protected virtual void ResolveMSAA(RenderDrawContext drawContext, Texture input, Texture output, int maxResolveSamples = (int)MSAALevel.X8)
        {
            if (MSAAResolver != null && MSAAResolver.Enabled)
            {
                MSAAResolver.Resolve(drawContext, input, output, maxResolveSamples);
            }
            else
            {
                drawContext.CommandList.CopyMultiSample(input, 0, output, 0);
            }
        }

        protected virtual void ResolveMSAA(RenderDrawContext drawContext)
        {
            TargetsCompositionNoMSAA.Copy(TargetsComposition);
            for (int i = 0; i < TargetsComposition.List.Count; ++i)
            {
                var inputTarget = TargetsComposition.List[i];
                var input = inputTarget.Texture;
                var output = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(TextureDescription.New2D(
                            input.ViewWidth, input.ViewHeight, 1, input.Format, TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                TargetsCompositionNoMSAA.SetTexture(inputTarget.Description.Semantic.GetType(), output);
                ResolveMSAA(drawContext, input, output);
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
                    //renderSystem.PlugTargets(drawContext, OpaqueRenderStage);

                    renderSystem.Draw(drawContext, context.RenderView, OpaqueRenderStage);
                }

                // Draw [main view | transparent stage]
                Texture depthStencilSRV = null;
                if (TransparentRenderStage != null)
                {
                    // Some transparent shaders will require the depth as a shader resource - resolve it only once and set it here
                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        depthStencilSRV = ResolveDepthAsSRV(drawContext);

                        // Override depth stencil buffer if it doesn't support SRV
                        if (depthStencilSRV != null)
                            ViewDepthStencil = depthStencilSRV;

                        renderSystem.Draw(drawContext, context.RenderView, TransparentRenderStage);
                    }
                }

                if (PostEffects != null)
                {
                    //Make sure we run post effects without MSAA
                    var renderTargets = TargetsComposition;
                    var depthStencil = ViewDepthStencil;

                    // Resolve MSAA targets
                    if (actualMSAALevel != MSAALevel.None)
                    {
                        // If lightprobes (which need Z-Prepass) are enabled, depth is already resolved
                        //if (!lightProbes)
                        // TODO: is that comment above true? i don't know lightprobes rendering stuff but if it does it should override ResolveDepthMSAA? maybe some redesign...
                        renderTargets = TargetsCompositionNoMSAA;
                        ResolveDepthMSAA(drawContext);
                        ResolveMSAA(drawContext);

                        depthStencil = ViewDepthStencilNoMSAA;
                    }

                    //Shafts if we have them
                    LightShafts?.Draw(drawContext, renderTargets, depthStencil, ViewOutputTarget);

                    // Run post effects
                    PostEffects.Draw(drawContext, renderTargets, depthStencil, ViewOutputTarget);
                }
                else
                {
                    if (actualMSAALevel != MSAALevel.None && TargetsComposition.IsActive(typeof(ColorTargetSemantic)))
                    {
                        var color = TargetsComposition.GetRenderTarget(typeof(ColorTargetSemantic)).Texture;
                        if (color != null)
                        {
                            ResolveMSAA(drawContext, color, ViewOutputTarget);
                        }
                    }
                }

                // Free the depth texture since we won't need it anymore
                if (depthStencilSRV != null)
                {
                    drawContext.Resolver.ReleaseDepthStenctilAsShaderResource(depthStencilSRV);
                }
            }
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

                        PrepareRenderTargetTextures(drawContext, new Size2(VRSettings.VRDevice.ActualRenderFrameSize.Width / 2, VRSettings.VRDevice.ActualRenderFrameSize.Height));

                        //also prepare the final VR target
                        var vrFullSurface = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                                                TextureDescription.New2D(VRSettings.VRDevice.ActualRenderFrameSize.Width, VRSettings.VRDevice.ActualRenderFrameSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                                                    TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
                     
                        //draw per eye
                        using (context.SaveViewportAndRestore())
                        using (drawContext.PushRenderTargetsAndRestore())
                        {
                            drawContext.CommandList.SetViewport(new Viewport(0.0f, 0.0f, VRSettings.VRDevice.ActualRenderFrameSize.Width / 2.0f, VRSettings.VRDevice.ActualRenderFrameSize.Height));
                            drawContext.CommandList.SetRenderTargets(ViewDepthStencil, TargetsComposition.TexturesComposition.Length, TargetsComposition.TexturesComposition);

                            TargetsComposition.ViewsCount = 2;

                            for (var i = 0; i < 2; i++)
                            {
                                using (context.PushRenderViewAndRestore(VRSettings.RenderViews[i]))
                                {
                                    // Clear render target and depth stencil
                                    Clear?.Draw(drawContext);

                                    TargetsComposition.ViewsIndex = i;

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
                    PrepareRenderTargetTextures(drawContext, new Size2((int)viewport.Width, (int)viewport.Height));

                    using (drawContext.PushRenderTargetsAndRestore())
                    {
                        drawContext.CommandList.SetRenderTargetsAndViewport(ViewDepthStencil, TargetsComposition.TexturesComposition.Length, TargetsComposition.TexturesComposition);

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

        /// <summary>
        /// Prepare the targets composition to allow effect permutations to be mixed in with the desired MRT output colorX class computers.
        /// </summary>
        protected virtual void CollectOpaqueStageRenderTargetComposition(RenderContext context)
        {
            TargetsComposition.Clear();

            TargetsComposition.AddTargetTo<ColorTargetSemantic>();

            if (PostEffects != null && PostEffects.RequiresNormalBuffer)
            {
                TargetsComposition.AddTargetTo<NormalTargetSemantic>();
            }

            if (PostEffects != null && PostEffects.RequiresVelocityBuffer)
            {
                TargetsComposition.AddTargetTo<VelocityTargetSemantic>();
            }

            if (PostEffects != null && PostEffects.RequiresSsrGBuffers)
            {
                TargetsComposition.AddTargetTo<OctaNormalSpecColorTargetSemantic>();
                TargetsComposition.AddTargetTo<EnvlightRoughnessTargetSemantic>();
            }
        }

        protected virtual void PrepareRenderTargetCreateParams(RenderDrawContext drawContext, Texture currentRenderTarget)
        {
            var colorParms = new RenderTargetTextureCreationParams
            {
                PixelFormat = PostEffects != null ? PixelFormat.R16G16B16A16_Float : currentRenderTarget.ViewFormat,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(ColorTargetSemantic), colorParms);

            var normalParams = new RenderTargetTextureCreationParams
            {
                PixelFormat = PixelFormat.R16G16B16A16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(NormalTargetSemantic), normalParams, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);

            var velocityParams = new RenderTargetTextureCreationParams
            {
                PixelFormat = PixelFormat.R16G16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(VelocityTargetSemantic), velocityParams, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);

            var rlrGBuffer1Params = new RenderTargetTextureCreationParams
            {
                PixelFormat = PixelFormat.R16G16B16A16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(OctaNormalSpecColorTargetSemantic), rlrGBuffer1Params, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);

            var rlrGBuffer2Params = new RenderTargetTextureCreationParams
            {
                PixelFormat = PixelFormat.R16G16B16A16_Float,
                MSAALevel = actualMSAALevel,
                TextureFlags = TextureFlags.ShaderResource
            };
            TargetsComposition.SetTextureParams(typeof(EnvlightRoughnessTargetSemantic), rlrGBuffer2Params, SetPolicy.DefendSilentlyIfSemanticKeyNotFound);
        }

        protected virtual Texture CreateRenderTargetTexture(RenderDrawContext drawContext, RenderTargetTextureCreationParams creationParams, int width, int height)
        {
            return PushScopedResource(
                drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(width, height, 1, creationParams.PixelFormat,
                        creationParams.TextureFlags | TextureFlags.RenderTarget, 1, GraphicsResourceUsage.Default,
                        creationParams.MSAALevel)));
        }

        protected virtual void CreateRegisteredRenderTargetTextures(RenderDrawContext drawContext, Texture currentRenderTarget)
        {
            foreach (var renderTarget in TargetsComposition.List)
            {
                bool canUseCurrent = renderTarget.Description.Semantic is ColorTargetSemantic &&
                    currentRenderTarget.Description.MultiSampleLevel == renderTarget.Description.RenderTargetTextureParams.MSAALevel &&
                    currentRenderTarget.Description.Format == renderTarget.Description.RenderTargetTextureParams.PixelFormat;

                var texture = canUseCurrent ? currentRenderTarget :
                    CreateRenderTargetTexture(drawContext, renderTarget.Description.RenderTargetTextureParams, currentRenderTarget.Width, currentRenderTarget.Height);

                TargetsComposition.SetTexture(renderTarget.Description.Semantic.GetType(), texture);
            }
        }

        /// <summary>
        /// Prepares targets per frame, caching and handling MSAA etc.
        /// </summary>
        /// <param name="drawContext">The current draw context</param>
        /// <param name="renderTargetsSize"></param>
        protected virtual void PrepareRenderTargetTextures(RenderDrawContext drawContext, Size2 renderTargetsSize)
        {
            var currentRenderTarget = drawContext.CommandList.RenderTarget;
            if (drawContext.CommandList.RenderTargetCount == 0)
                currentRenderTarget = null;

            var currentDepthStencil = drawContext.CommandList.DepthStencilBuffer;

            // Make sure we got a valid NOT MSAA final OUTPUT Target
            if (currentRenderTarget == null)
            {
                currentRenderTarget = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb,
                        TextureFlags.ShaderResource | TextureFlags.RenderTarget)));
            }

            PrepareRenderTargetCreateParams(drawContext, currentRenderTarget);

            CreateRegisteredRenderTargetTextures(drawContext, currentRenderTarget);

            //MSAA, we definitely need new buffers
            if (actualMSAALevel != MSAALevel.None)
            {
                //Handle Depth
                ViewDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                    TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, currentDepthStencil?.ViewFormat ?? PixelFormat.D24_UNorm_S8_UInt,
                        TextureFlags.ShaderResource | TextureFlags.DepthStencil, 1, GraphicsResourceUsage.Default, actualMSAALevel)));
            }
            else
            {
                //Handle Depth
                if (currentDepthStencil == null)
                {
                    currentDepthStencil = PushScopedResource(drawContext.GraphicsContext.Allocator.GetTemporaryTexture2D(
                        TextureDescription.New2D(renderTargetsSize.Width, renderTargetsSize.Height, 1, PixelFormat.D24_UNorm_S8_UInt,
                            TextureFlags.ShaderResource | TextureFlags.DepthStencil)));
                }
                ViewDepthStencil = currentDepthStencil;
            }

            ViewOutputTarget = currentRenderTarget;
        }

        protected override void Destroy()
        {
            PostEffects?.Dispose();
        }
    }
}
