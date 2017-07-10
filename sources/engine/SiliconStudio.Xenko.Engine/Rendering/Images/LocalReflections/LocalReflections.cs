#if DEBUG
// Enables/disables Screen Space Local Reflections effect debugging
#define SSLR_DEBUG
#endif

// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Compute screen space reflections as a post effect.
    /// </summary>
    [DataContract("LocalReflections")]
    public sealed class LocalReflections : ImageEffect
    {
        private ImageEffectShader blurPassShader;
        private ImageEffectShader rayTracePassShader;
        private ImageEffectShader resolvePassShader;
        private ImageEffectShader temporalPassShader;
        private ImageEffectShader combinePassShader;

        public Texture blueNoiseTexture;
        private Texture temporalBuffer;

        private Texture[] cachedColorBuffer0Mips;
        private Texture[] cachedColorBuffer1Mips;

        [DataContract("ResolutionMode")]
        public enum ResolutionMode
        {
            /// <summary>
            /// Use full resolution.
            /// </summary>
            /// <userodc>Full resolution.</userodc>
            [Display("Full")]
            Full = 1,

            /// <summary>
            /// Use hald resolution.
            /// </summary>
            /// <userodc>Half resolution.</userodc>
            [Display("Half")]
            Half = 2
        }

        /// <summary>
        /// Gets or sets the input depth resolution mode.
        /// </summary>
        [Display("Depth resolution")]
        [DefaultValue(ResolutionMode.Full)]
        public ResolutionMode DepthResolution { get; set; } = ResolutionMode.Full;

        /// <summary>
        /// Gets or sets the ray trace pass resolution mode.
        /// </summary>
        [Display("Ray trace pass resolution")]
        [DefaultValue(ResolutionMode.Half)]
        public ResolutionMode RayTracePassResolution { get; set; } = ResolutionMode.Half;   
        
        /// <summary>
        /// Gets or sets the resolve pass resolution mode.
        /// </summary>
        [Display("Resolve pass resolution")]
        [DefaultValue(ResolutionMode.Half)]
        public ResolutionMode ResolvePassResolution { get; set; } = ResolutionMode.Full;

        /// <summary>
        /// Maximum allowed amount of dynamic iterations in the ray trace pass.
        /// Higher value provides better ray tracing quality but decreases the performance.
        /// Default value is 60.
        /// </summary>
        [Display("Max steps amount")]
        [DefaultValue(60)]
        public int MaxStepsAmount { get; set; } = 60;

        /// <summary>
        /// Maximum allowed surface roughness value to use local reflections.
        /// Pixels with higher values won't be affected by the effect.
        /// </summary>
        [Display("Max roughness")]
        [DefaultValue(0.6f)]
        public float MaxRoughness { get; set; } = 0.6f;

        /// <summary>
        /// Ray tracing starting position is offseted by a percent of the normal in world space to avoid self occlusions.
        /// </summary>
        [Display("Ray start bias")]
        [DefaultValue(0.01f)]
        public float WorldAntiSelfOcclusionBias { get; set; } = 0.01f;

        /// <summary>
        /// Gets or sets the resolve pass samples amount. Higher values provide better quality but reduce effect performance.
        /// Default value is 4. Use 1 for the highest speed.
        /// </summary>
        /// <value>
        /// The resolve samples amount.
        /// </value>
        [Display("Resolve Samples")]
        [DefaultValue(4)]
        public int ResolveSamples { get; set; } = 4;

        /// <summary>
        /// Gets or sets a value indicating whether reduce fireflies during resolve pass.
        /// Performs filtering on sampled pixels to smooth luminance bursts.
        /// It helps to provide softer image with reduced amount of highlights.
        /// </summary>
        /// <value>
        ///   <c>true</c> if reduce fireflies; otherwise, <c>false</c>.
        /// </value>
        [Display("Reduce Fireflies")]
        [DefaultValue(true)]
        public bool ReduceFireflies { get; set; } = true;

        public bool UseColorBufferMips { get; set; } = false; // use true later, test perf diff on 4k (resolve should pass run faster but check it)

        // TODO: add docs
        public float BRDFBias { get; set; } = 0.7f;
        public bool UseTemporal { get; set; } = true;
        public float TemporalScale { get; set; } = 1.5f;
        public float TemporalResponse { get; set; } = 0.85f;
        
#if SSLR_DEBUG

        public enum DebugModes
        {
            None,
            RayTrace,
            RayTraceMask,
            Resolve,
            Temporal,
        }

        public DebugModes DebugMode = DebugModes.RayTrace;

#endif

        protected override void InitializeCore()
        {
            base.InitializeCore();

            blurPassShader = ToLoadAndUnload(new ImageEffectShader("SSLRBlurPassEffect"));
            rayTracePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRRayTracePass"));
            resolvePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRResolvePassEffect"));
            temporalPassShader = ToLoadAndUnload(new ImageEffectShader("SSLRTemporalPass"));
            combinePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRCombinePass"));

            Texture obj = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("aF02239B-3697-4EBB-9F37-FE880659E64B"), "BlueNoise_256x256_UNI");
            string url = AttachedReferenceManager.GetUrl(obj);

            //blueNoiseTexture = AttachedReferenceManager.CreateProxyObject<Texture>(new AssetId("aF02239B-3697-4EBB-9F37-FE880659E64B"), "BlueNoise_256x256_UNI");
            //blueNoiseTexture = Content.Load<Texture>("BlueNoise_256x256_UNI");
            //blueNoiseTexture = Content.Load<Texture>(url);
        }

        protected override void Destroy()
        {
            if (temporalBuffer != null)
            {
                temporalBuffer.Dispose();
                temporalBuffer = null;
            }

            cachedColorBuffer0Mips?.ForEach(view => view?.Dispose());
            cachedColorBuffer1Mips?.ForEach(view => view?.Dispose());

            base.Destroy();
        }

        private Size3 GetBufferResolution(Texture fullResTarget, ResolutionMode mode)
        {
            return new Size3(fullResTarget.Width / (int)mode, fullResTarget.Height / (int)mode, 1);
        }
        
        /// <summary>
        /// Provides a color buffer and a depth buffer to apply the depth-of-field to.
        /// </summary>
        /// <param name="colorBuffer">Single view of the scene</param>
        /// <param name="depthBuffer">The depth buffer corresponding to the color buffer provided.</param>
        /// <param name="normalsBuffer">The buffer which contains surface packed world space normal vectors.</param>
        /// <param name="specularRoughnessBuffer">The buffer which contains surface specular color and roughness.</param>
        public void SetInputSurfaces(Texture colorBuffer, Texture depthBuffer, Texture normalsBuffer, Texture specularRoughnessBuffer)
        {
            SetInput(0, colorBuffer);
            SetInput(1, depthBuffer);
            SetInput(2, normalsBuffer);
            SetInput(3, specularRoughnessBuffer);
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            Texture outputBuffer = GetSafeOutput(0);

            if (!blurPassShader.Initialized)
                blurPassShader.Initialize(context.RenderContext);
            if (!rayTracePassShader.Initialized)
                rayTracePassShader.Initialize(context.RenderContext);
            if (!resolvePassShader.Initialized)
                resolvePassShader.Initialize(context.RenderContext);
            if (!temporalPassShader.Initialized)
                temporalPassShader.Initialize(context.RenderContext);
            if (!combinePassShader.Initialized)
                combinePassShader.Initialize(context.RenderContext);

            // TODO: cleanup that stuff

            var currentCamera = context.RenderContext.GetCurrentCamera();
            if (currentCamera == null)
                throw new InvalidOperationException("No valid camera");
            Matrix viewMatrix = currentCamera.ViewMatrix;
            Matrix projectionMatrix = currentCamera.ProjectionMatrix;
            Matrix viewProjectionMatrix = currentCamera.ViewProjectionMatrix;
            Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
            Matrix inverseProjectionMatrix = Matrix.Invert(projectionMatrix);
            Matrix inverseViewProjectionMatrix = Matrix.Invert(viewProjectionMatrix);
            Vector4 eye = inverseViewMatrix.Row4;
            float nearclip = currentCamera.NearClipPlane;
            float farclip = currentCamera.FarClipPlane;
            float aspect = currentCamera.AspectRatio;
            float fieldOfView = (float)(2.0f * Math.Atan2(projectionMatrix.M11, aspect));
            Vector4 ViewInfo = new Vector4(1.0f / projectionMatrix.M11, 1.0f / projectionMatrix.M22, farclip / (farclip - nearclip), (-farclip * nearclip) / (farclip - nearclip) / farclip);
            Vector4 CameraPosWS = new Vector4(eye.X, eye.Y, eye.Z, WorldAntiSelfOcclusionBias);
            
            float time = (float)(context.RenderContext.Time.Total.TotalSeconds);

            var traceBufferSize = GetBufferResolution(outputBuffer, RayTracePassResolution);
            Vector2 ScreenSize = new Vector2(traceBufferSize.Width, traceBufferSize.Height);
            var roughnessFade = MathUtil.Clamp(MaxRoughness, 0.0f, 1.0f);
            var maxTraceSamples = MathUtil.Clamp(MaxStepsAmount, 1, 128);

            // ViewInfo    :  x-1/Projection[0,0]   y-1/Projection[1,1]   z-(Far / (Far - Near)   w-(-Far * Near) / (Far - Near) / Far)

            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ViewInfo);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.MaxTraceSamples, maxTraceSamples);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, CameraPosWS);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ScreenSize, ScreenSize);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.RayStepScale, 2.0f / outputBuffer.Width);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.Time, time);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.BRDFBias, BRDFBias);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.UseTemporal, UseTemporal ? 1 : 0);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.TemporalResponse, TemporalResponse);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.TemporalScale, TemporalScale);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.P, projectionMatrix);

            resolvePassShader.Parameters.Set(SSLRCommonKeys.MaxColorMiplevel, Texture.CalculateMipMapCount(0, outputBuffer.Width, outputBuffer.Height) - 1);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.TraceSizeMax, Math.Max(traceBufferSize.Width, traceBufferSize.Height) / 2.0f);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.SSRtexelSize, new Vector2(1.0f / traceBufferSize.Width, 1.0f / traceBufferSize.Height));
            resolvePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ViewInfo);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.MaxTraceSamples, maxTraceSamples);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, CameraPosWS);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.ScreenSize, ScreenSize);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.RayStepScale, 2.0f / outputBuffer.Width);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.Time, time);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.BRDFBias, BRDFBias);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.UseTemporal, UseTemporal ? 1 : 0);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.TemporalResponse, TemporalResponse);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.TemporalScale, TemporalScale);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);
            resolvePassShader.Parameters.Set(SSLRKeys.ResolveSamples, MathUtil.Clamp(ResolveSamples, 1, 8));
            resolvePassShader.Parameters.Set(SSLRKeys.ReduceFireflies, ReduceFireflies);

            temporalPassShader.Parameters.Set(SSLRTemporalPassKeys.TemporalResponse, TemporalResponse);
            temporalPassShader.Parameters.Set(SSLRTemporalPassKeys.TemporalScale, TemporalScale);

            combinePassShader.Parameters.Set(SSLRCommonKeys.MaxColorMiplevel, Texture.CalculateMipMapCount(0, outputBuffer.Width, outputBuffer.Height) - 1);
            combinePassShader.Parameters.Set(SSLRCommonKeys.TraceSizeMax, Math.Max(traceBufferSize.Width, traceBufferSize.Height) / 2.0f);
            combinePassShader.Parameters.Set(SSLRCommonKeys.SSRtexelSize, new Vector2(1.0f / traceBufferSize.Width, 1.0f / traceBufferSize.Height));
            combinePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ViewInfo);
            combinePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            combinePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            combinePassShader.Parameters.Set(SSLRCommonKeys.MaxTraceSamples, maxTraceSamples);
            combinePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, eye);
            combinePassShader.Parameters.Set(SSLRCommonKeys.ScreenSize, ScreenSize);
            combinePassShader.Parameters.Set(SSLRCommonKeys.RayStepScale, 2.0f / outputBuffer.Width);
            combinePassShader.Parameters.Set(SSLRCommonKeys.Time, time);
            combinePassShader.Parameters.Set(SSLRCommonKeys.BRDFBias, BRDFBias);
            combinePassShader.Parameters.Set(SSLRCommonKeys.UseTemporal, UseTemporal ? 1 : 0);
            combinePassShader.Parameters.Set(SSLRCommonKeys.TemporalResponse, TemporalResponse);
            combinePassShader.Parameters.Set(SSLRCommonKeys.TemporalScale, TemporalScale);
            combinePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            combinePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            combinePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            //if(blueNoiseTexture == null)
            //    blueNoiseTexture = Content.Load<Texture>("BlueNoise_256x256_UNI");

            // Inputs:
            Texture colorBuffer = GetSafeInput(0);
            Texture depthBuffer = GetSafeInput(1);
            Texture normalsBuffer = GetSafeInput(2);
            Texture specularRoughnessBuffer = GetSafeInput(3);

            // Output:
            Texture outputBuffer = GetSafeOutput(0);

            // Get temporary buffers
            // TODO: try optimize formats
            var reflectionsFormat = PixelFormat.R16G16B16A16_Float;
            var rayTraceBuffersSize = GetBufferResolution(outputBuffer, RayTracePassResolution);
            var resolveBuffersSize = GetBufferResolution(outputBuffer, ResolvePassResolution);
            Texture rayTraceBuffer = NewScopedRenderTarget2D(rayTraceBuffersSize.Width, rayTraceBuffersSize.Height, PixelFormat.R16G16B16A16_Float, 1);
            Texture rayTraceMaskBuffer = NewScopedRenderTarget2D(rayTraceBuffersSize.Width, rayTraceBuffersSize.Height, PixelFormat.R16_Float, 1);
            Texture resolveBuffer = NewScopedRenderTarget2D(resolveBuffersSize.Width, resolveBuffersSize.Height, reflectionsFormat, 1);

            // Check if resize depth
            if (DepthResolution != ResolutionMode.Full)
            {
                var depthBuffersSize = GetBufferResolution(outputBuffer, DepthResolution);

                // TODO: use half res depth as default
                throw new NotImplementedException("finish depth downscale");
                //Texture smallerDepth = NewScopedRenderTarget2D(depthBuffersSize.Width, depthBuffersSize.Height, PixelFormat.R32_Float, 1);
            }
            
            // Blur Pass
            Texture blurPassBuffer;
            if (UseColorBufferMips)
            {
                // Note: using color buffer mips maps helps with reducing artifacts
                // and improves resolve pass performance (faster color texture lookups, less cache misses)
                // Also for high surface roughness values it adds more blur to the reflection tail which looks more realistic.
                
                // Get temp targets
                var colorBuffersSize = new Size2(outputBuffer.Width / 2, outputBuffer.Height / 2);
                Texture colorBuffer0 = NewScopedRenderTarget2D(colorBuffersSize.Width, colorBuffersSize.Height, PixelFormat.R11G11B10_Float, MipMapCount.Auto);
                Texture colorBuffer1 = NewScopedRenderTarget2D(colorBuffersSize.Width, colorBuffersSize.Height, PixelFormat.R11G11B10_Float, MipMapCount.Auto);
                // TODO: we don't use colorBuffer1 mip0, could be optimized

                // Cache per color buffer mip views
                int colorMipLevels = colorBuffer0.MipLevels;
                if (cachedColorBuffer0Mips == null || cachedColorBuffer0Mips.Length != colorMipLevels || cachedColorBuffer0Mips[0].ParentTexture != colorBuffer0)
                {
                    cachedColorBuffer0Mips?.ForEach(view => view?.Dispose());
                    cachedColorBuffer0Mips = new Texture[colorMipLevels];
                    for (int mipIndex = 0; mipIndex < colorMipLevels; mipIndex++)
                    {
                        cachedColorBuffer0Mips[mipIndex] = colorBuffer0.ToTextureView(ViewType.Single, 0, mipIndex);
                    }
                }
                if (cachedColorBuffer1Mips == null || cachedColorBuffer1Mips.Length != colorMipLevels || cachedColorBuffer1Mips[0].ParentTexture != colorBuffer1)
                {
                    cachedColorBuffer1Mips?.ForEach(view => view?.Dispose());
                    cachedColorBuffer1Mips = new Texture[colorMipLevels];
                    for (int mipIndex = 0; mipIndex < colorMipLevels; mipIndex++)
                    {
                        cachedColorBuffer1Mips[mipIndex] = colorBuffer1.ToTextureView(ViewType.Single, 0, mipIndex);
                    }
                }

                // Clone scene frame to mip 0 of colorBuffer0
                Scaler.SetInput(0, colorBuffer);
                Scaler.SetOutput(cachedColorBuffer0Mips[0]);
                Scaler.Draw(context, "Copy frame");

                // Downscale with gaussian blur
                for (int mipLevel = 1; mipLevel < colorMipLevels; mipLevel++)
                {
                    // Blur H
                    //var srcMip = mipLevel == 0 ? cachedColorBuffer0Mips[0] : cachedColorBuffer0Mips[mipLevel - 1];
                    var srcMip = cachedColorBuffer0Mips[mipLevel - 1];
                    var dstMip = cachedColorBuffer1Mips[mipLevel];
                    blurPassShader.SetInput(0, srcMip);
                    blurPassShader.SetOutput(dstMip);
                    blurPassShader.Parameters.Set(SSLRBlurPassParams.ConvolveVertical, 0);
                    blurPassShader.Draw(context, "Blur H");

                    // Blur V
                    srcMip = dstMip;
                    dstMip = cachedColorBuffer0Mips[mipLevel];
                    blurPassShader.SetInput(0, srcMip);
                    blurPassShader.SetOutput(dstMip);
                    blurPassShader.Parameters.Set(SSLRBlurPassParams.ConvolveVertical, 1);
                    blurPassShader.Draw(context, "Blur V");
                }

                blurPassBuffer = colorBuffer0;
            }
            else
            {
                // Don't use color buffer with mip maps
                blurPassBuffer = colorBuffer;
                
                cachedColorBuffer0Mips?.ForEach(view => view?.Dispose());
                cachedColorBuffer1Mips?.ForEach(view => view?.Dispose());
            }

            // Ray Trace Pass
            rayTracePassShader.SetInput(0, colorBuffer);
            rayTracePassShader.SetInput(1, depthBuffer);
            rayTracePassShader.SetInput(2, normalsBuffer);
            rayTracePassShader.SetInput(3, specularRoughnessBuffer);
            rayTracePassShader.SetInput(4, blueNoiseTexture);
            rayTracePassShader.SetOutput(rayTraceBuffer, rayTraceMaskBuffer);
            rayTracePassShader.Draw(context, "Ray Trace");

            // Resolve Pass
            resolvePassShader.SetInput(0, blurPassBuffer);
            resolvePassShader.SetInput(1, depthBuffer);
            resolvePassShader.SetInput(2, normalsBuffer);
            resolvePassShader.SetInput(3, specularRoughnessBuffer);
            resolvePassShader.SetInput(4, blueNoiseTexture);
            resolvePassShader.SetInput(5, rayTraceBuffer);
            resolvePassShader.SetInput(6, rayTraceMaskBuffer);
            resolvePassShader.SetOutput(resolveBuffer);
            resolvePassShader.Draw(context, "Resolve");

            // Temporal Pass
            Texture reflectionsBuffer = resolveBuffer;
            if (UseTemporal)
            {
                var temporalSize = outputBuffer.Size;
                if (temporalBuffer == null || temporalBuffer.Size != temporalSize)
                {
                    if (temporalBuffer != null)
                        temporalBuffer.Dispose();
                    temporalBuffer = Texture.New2D(GraphicsDevice, temporalSize.Width, temporalSize.Height, 1, reflectionsFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                }

                Texture temporalBuffer0 = NewScopedRenderTarget2D(temporalSize.Width, temporalSize.Height, reflectionsFormat, 1);

                temporalPassShader.SetInput(0, resolveBuffer);
                temporalPassShader.SetInput(1, temporalBuffer);
                temporalPassShader.SetOutput(temporalBuffer0);
                temporalPassShader.Draw(context, "Temporal");

                context.CommandList.Copy(temporalBuffer0, temporalBuffer); // TODO: use Texture.Swap from ContentStreaming branch to make it faster!

                reflectionsBuffer = temporalBuffer;
            }

            // Combine Pass
            combinePassShader.SetInput(0, colorBuffer);
            combinePassShader.SetInput(1, depthBuffer);
            combinePassShader.SetInput(2, normalsBuffer);
            combinePassShader.SetInput(3, specularRoughnessBuffer);
            combinePassShader.SetInput(4, reflectionsBuffer);
            combinePassShader.SetOutput(outputBuffer);
            combinePassShader.Draw(context, "Combine");

#if SSLR_DEBUG
            if (DebugMode != DebugModes.None)
            {
                // Debug preview of temp targets
                switch (DebugMode)
                {
                    case DebugModes.RayTrace:
                        Scaler.SetInput(0, rayTraceBuffer);
                        break;
                    case DebugModes.RayTraceMask:
                        Scaler.SetInput(0, rayTraceMaskBuffer);
                        break;
                    case DebugModes.Resolve:
                        Scaler.SetInput(0, resolveBuffer);
                        break;
                    case DebugModes.Temporal:
                        Scaler.SetInput(0, temporalBuffer);
                        break;
                }
                Scaler.SetOutput(outputBuffer);
                Scaler.Draw(context);
            }
#endif
        }
    }
}