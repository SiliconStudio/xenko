#if DEBUG
// Enables/disables Screen Space Local Reflections effect debugging
#define SSLR_DEBUG
#endif

// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Compute screen space reflections as a post effect.
    /// </summary>
    [DataContract("LocalReflections")]
    public sealed class LocalReflections : ImageEffect
    {
        // Short description:
        // The following implementation is using Stochastic Screen-Space Reflections algorithm based on:
        // https://www.slideshare.net/DICEStudio/stochastic-screenspace-reflections
        // It's well optimized and provides solid visual effect.
        //
        // Algorithm steps:
        // 1) Downscale depth [optional]
        // 2) Ray trace
        // 3) Resolve rays
        // 4) Temporal blur [optional]
        // 5) Combine final image
        
        private ImageEffectShader depthPassShader;
        private ImageEffectShader blurPassShaderH;
        private ImageEffectShader blurPassShaderV;
        private ImageEffectShader rayTracePassShader;
        private ImageEffectShader resolvePassShader;
        private ImageEffectShader temporalPassShader;
        private ImageEffectShader combinePassShader;

        private Texture temporalBuffer;

        // ReSharper disable once InconsistentNaming
        private Matrix prevVP;

        private Texture[] cachedColorBuffer0Mips;
        private Texture[] cachedColorBuffer1Mips;

        [DataContract("ResolutionMode")]
        public enum ResolutionMode
        {
            /// <summary>
            /// Use full resolution.
            /// </summary>
            /// <userodc>Full resolution.</userodc>
            [Display("Full")] Full = 1,

            /// <summary>
            /// Use hald resolution.
            /// </summary>
            /// <userodc>Half resolution.</userodc>
            [Display("Half")] Half = 2
        }

        /// <summary>
        /// Gets or sets the input depth resolution mode.
        /// </summary>
        [Display("Depth resolution")]
        [DefaultValue(ResolutionMode.Full)]
        public ResolutionMode DepthResolution { get; set; } = ResolutionMode.Half;

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
        [DataMemberRange(1, 128, 1, 10, 0)]
        public int MaxStepsAmount { get; set; } = 60;

        /// <summary>
        /// Maximum allowed surface roughness value to use local reflections.
        /// Pixels with higher values won't be affected by the effect.
        /// </summary>
        [Display("Max roughness")]
        [DefaultValue(0.45f)]
        [DataMemberRange(0.0, 1.0, 0.05, 0.2, 4)]
        public float MaxRoughness { get; set; } = 0.45f;

        /// <summary>
        /// Ray tracing starting position is offseted by a percent of the normal in world space to avoid self occlusions.
        /// </summary>
        [Display("Ray start bias")]
        [DefaultValue(0.01f)]
        [DataMemberRange(0.0, 0.1, 0.005, 0.01, 6)]
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
        [DataMemberRange(1, 8, 1, 1, 0)]
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

        /// <summary>
        /// Gets or sets the edge fade factor. It's used to fade off effect on screen edges to provide smoother image.
        /// </summary>
        /// <value>
        /// The edge fade factor.
        /// </value>
        [Display("Edge Fade Factor")]
        [DefaultValue(0.2f)]
        [DataMemberRange(0.0, 1.0, 0.05, 0.2, 4)]
        public float EdgeFadeFactor { get; set; } = 0.2f;

        /// <summary>
        /// Gets or sets a value indicating whether use color buffer mipsmaps chain; otherwise will use raw input color buffer to sample reflections color.
        /// Using mipmaps improves resolve pass performance and reduces GPU cache misses.
        /// </summary>
        /// <value>
        ///   <c>true</c> if use color buffer mips; otherwise, <c>false</c>.
        /// </value>
        [Display("Use Color Buffer Mips")]
        [DefaultValue(true)]
        public bool UseColorBufferMips { get; set; } = true;

        /// <summary>
        /// Gets or sets the BRDF bias. This value controlls source roughness effect on reflections blur.
        /// Smaller values produce wider reflections spread but also introduce more noise.
        /// Higher values provide more mirror-like reflections. Default value is 0.8.
        /// </summary>
        /// <value>
        /// The BRDF bias.
        /// </value>
        [Display("BRDF Bias")]
        [DefaultValue(0.8f)]
        [DataMemberRange(0.0, 1.0, 0.05, 0.2, 4)]
        // ReSharper disable once InconsistentNaming
        public float BRDFBias { get; set; } = 0.8f;

        /// <summary>
        /// Gets or sets a value indicating whether use temporal effect to smooth reflections.
        /// </summary>
        /// <value>
        ///   <c>true</c> if use temporal effect to smooth reflections; otherwise, <c>false</c>.
        /// </value>
        [Display("Temporal Enable")]
        [DefaultValue(true)]
        public bool TemporalEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the temporal effect scale. Default is 2.
        /// </summary>
        /// <value>
        /// The temporal effect scale.
        /// </value>
        [Display("Temporal Scale")]
        [DefaultValue(1.5f)]
        [DataMemberRange(0.0, 20.0, 0.5, 0.5, 2)]
        public float TemporalScale { get; set; } = 2.0f;

        /// <summary>
        /// Gets or sets the temporal response. Default is 0.9.
        /// </summary>
        /// <value>
        /// The temporal response.
        /// </value>
        [Display("Temporal Response")]
        [DefaultValue(0.9f)]
        [DataMemberRange(0.5, 1.0, 0.01, 0.1, 2)]
        public float TemporalResponse { get; set; } = 0.9f;

#if SSLR_DEBUG

        public enum DebugModes
        {
            None,
            RayTrace,
            Resolve,
            Temporal,
        }

        public DebugModes DebugMode = DebugModes.None;

#endif

        protected override void InitializeCore()
        {
            base.InitializeCore();

            depthPassShader = ToLoadAndUnload(new ImageEffectShader("SSLRDepthPass"));
            blurPassShaderH = ToLoadAndUnload(new ImageEffectShader("SSLRBlurPassEffectH"));
            blurPassShaderV = ToLoadAndUnload(new ImageEffectShader("SSLRBlurPassEffectV"));
            rayTracePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRRayTracePass"));
            resolvePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRResolvePassEffect"));
            temporalPassShader = ToLoadAndUnload(new ImageEffectShader("SSLRTemporalPass"));
            combinePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRCombinePass"));
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

            var currentCamera = context.RenderContext.GetCurrentCamera();
            if (currentCamera == null)
                throw new InvalidOperationException("No valid camera");
            Matrix viewMatrix = currentCamera.ViewMatrix;
            Matrix projectionMatrix = currentCamera.ProjectionMatrix;
            Matrix viewProjectionMatrix = currentCamera.ViewProjectionMatrix;
            Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
            Matrix inverseViewProjectionMatrix = Matrix.Invert(viewProjectionMatrix);
            Vector4 eye = inverseViewMatrix.Row4;
            float nearclip = currentCamera.NearClipPlane;
            float farclip = currentCamera.FarClipPlane;
            Vector4 viewInfo = new Vector4(1.0f / projectionMatrix.M11, 1.0f / projectionMatrix.M22, farclip / (farclip - nearclip), (-farclip * nearclip) / (farclip - nearclip) / farclip);
            Vector3 cameraPos = new Vector3(eye.X, eye.Y, eye.Z);

            float temporalTime = 0;
            if (TemporalEnabled)
            {
                double time = context.RenderContext.Time.Total.TotalSeconds;

                // Keep time in smaller range to prevent temporal noise errors
                const double scale = 10;
                double integral = Math.Round(time / scale) * scale;
                time -= integral;

                temporalTime = (float)time;
            }

            var traceBufferSize = GetBufferResolution(outputBuffer, RayTracePassResolution);
            var roughnessFade = MathUtil.Clamp(MaxRoughness, 0.0f, 1.0f);
            var maxTraceSamples = MathUtil.Clamp(MaxStepsAmount, 1, 128);

            // ViewInfo :  x-1/Projection[0,0]   y-1/Projection[1,1]   z-(Far / (Far - Near)   w-(-Far * Near) / (Far - Near) / Far)

            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.MaxTraceSamples, maxTraceSamples);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.WorldAntiSelfOcclusionBias, WorldAntiSelfOcclusionBias);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.BRDFBias, BRDFBias);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.TemporalTime, temporalTime);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, ref cameraPos);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ref viewInfo);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.V, ref viewMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.IVP, ref inverseViewProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRRayTracePassKeys.VP, ref viewProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRRayTracePassKeys.EdgeFadeFactor, EdgeFadeFactor);

            resolvePassShader.Parameters.Set(SSLRCommonKeys.MaxColorMiplevel, Texture.CalculateMipMapCount(0, outputBuffer.Width, outputBuffer.Height) - 1);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.TraceSizeMax, Math.Max(traceBufferSize.Width, traceBufferSize.Height) / 2.0f);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.TemporalTime, temporalTime);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.BRDFBias, BRDFBias);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, ref cameraPos);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ref viewInfo);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.V, ref viewMatrix);
            resolvePassShader.Parameters.Set(SSLRCommonKeys.IVP, ref inverseViewProjectionMatrix);
            resolvePassShader.Parameters.Set(SSLRKeys.ResolveSamples, MathUtil.Clamp(ResolveSamples, 1, 8));
            resolvePassShader.Parameters.Set(SSLRKeys.ReduceFireflies, ReduceFireflies);

            if (TemporalEnabled)
            {
                temporalPassShader.Parameters.Set(SSLRTemporalPassKeys.IVP, ref inverseViewProjectionMatrix);
                temporalPassShader.Parameters.Set(SSLRTemporalPassKeys.prevVP, ref prevVP);
                temporalPassShader.Parameters.Set(SSLRTemporalPassKeys.TemporalResponse, TemporalResponse);
                temporalPassShader.Parameters.Set(SSLRTemporalPassKeys.TemporalScale, TemporalScale);
            }
            prevVP = viewProjectionMatrix;

            combinePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            combinePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, ref cameraPos);
            combinePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ref viewInfo);
            combinePassShader.Parameters.Set(SSLRCommonKeys.V, ref viewMatrix);
            combinePassShader.Parameters.Set(SSLRCommonKeys.IVP, ref inverseViewProjectionMatrix);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Inputs:
            Texture colorBuffer = GetSafeInput(0);
            Texture depthBuffer = GetSafeInput(1);
            Texture normalsBuffer = GetSafeInput(2);
            Texture specularRoughnessBuffer = GetSafeInput(3);

            // Output:
            Texture outputBuffer = GetSafeOutput(0);

            // Get temporary buffers
            var reflectionsFormat = PixelFormat.R11G11B10_Float;
            var rayTraceBuffersSize = GetBufferResolution(outputBuffer, RayTracePassResolution);
            var resolveBuffersSize = GetBufferResolution(outputBuffer, ResolvePassResolution);
            Texture rayTraceBuffer = NewScopedRenderTarget2D(rayTraceBuffersSize.Width, rayTraceBuffersSize.Height, PixelFormat.R11G11B10_Float, 1);
            Texture resolveBuffer = NewScopedRenderTarget2D(resolveBuffersSize.Width, resolveBuffersSize.Height, reflectionsFormat, 1);

            // Check if resize depth
            Texture smallerDepthBuffer = depthBuffer;
            if (DepthResolution != ResolutionMode.Full)
            {
                // Smaller depth buffer improves ray tracing performance.
                
                var depthBuffersSize = GetBufferResolution(depthBuffer, DepthResolution);
                smallerDepthBuffer = NewScopedRenderTarget2D(depthBuffersSize.Width, depthBuffersSize.Height, PixelFormat.R32_Float, 1);

                depthPassShader.SetInput(0, depthBuffer);
                depthPassShader.SetOutput(smallerDepthBuffer);
                depthPassShader.Draw(context, "Downscale Depth");
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
                int colorBuffersMips = Texture.CalculateMipMapCount(MipMapCount.Auto, colorBuffersSize.Width, colorBuffersSize.Height);
                Texture colorBuffer0 = NewScopedRenderTarget2D(colorBuffersSize.Width, colorBuffersSize.Height, reflectionsFormat, colorBuffersMips);
                Texture colorBuffer1 = NewScopedRenderTarget2D(colorBuffersSize.Width / 2, colorBuffersSize.Height / 2, reflectionsFormat, colorBuffersMips - 1);
                int colorBuffer1MipOffset = 1; // For colorBuffer1 we could use one mip less (optimized)

                // Cache per color buffer mip views
                int colorBuffer0Mips = colorBuffer0.MipLevels;
                if (cachedColorBuffer0Mips == null || cachedColorBuffer0Mips.Length != colorBuffer0Mips || cachedColorBuffer0Mips[0].ParentTexture != colorBuffer0)
                {
                    cachedColorBuffer0Mips?.ForEach(view => view?.Dispose());
                    cachedColorBuffer0Mips = new Texture[colorBuffer0Mips];
                    for (int mipIndex = 0; mipIndex < colorBuffer0Mips; mipIndex++)
                    {
                        cachedColorBuffer0Mips[mipIndex] = colorBuffer0.ToTextureView(ViewType.Single, 0, mipIndex);
                    }
                }
                int colorBuffer1Mips = colorBuffer1.MipLevels;
                if (cachedColorBuffer1Mips == null || cachedColorBuffer1Mips.Length != colorBuffer1Mips || cachedColorBuffer1Mips[0].ParentTexture != colorBuffer1)
                {
                    cachedColorBuffer1Mips?.ForEach(view => view?.Dispose());
                    cachedColorBuffer1Mips = new Texture[colorBuffer1Mips];
                    for (int mipIndex = 0; mipIndex < colorBuffer1Mips; mipIndex++)
                    {
                        cachedColorBuffer1Mips[mipIndex] = colorBuffer1.ToTextureView(ViewType.Single, 0, mipIndex);
                    }
                }

                // Clone scene frame to mip 0 of colorBuffer0
                Scaler.SetInput(0, colorBuffer);
                Scaler.SetOutput(cachedColorBuffer0Mips[0]);
                Scaler.Draw(context, "Copy frame");

                // Downscale with gaussian blur
                for (int mipLevel = 1; mipLevel < colorBuffersMips; mipLevel++)
                {
                    // Blur H
                    var srcMip = cachedColorBuffer0Mips[mipLevel - 1];
                    var dstMip = cachedColorBuffer1Mips[mipLevel - colorBuffer1MipOffset];
                    blurPassShaderH.SetInput(0, srcMip);
                    blurPassShaderH.SetOutput(dstMip);
                    blurPassShaderH.Draw(context, "Blur H");

                    // Blur V
                    srcMip = dstMip;
                    dstMip = cachedColorBuffer0Mips[mipLevel];
                    blurPassShaderV.SetInput(0, srcMip);
                    blurPassShaderV.SetOutput(dstMip);
                    blurPassShaderV.Draw(context, "Blur V");
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
            rayTracePassShader.SetInput(1, smallerDepthBuffer);
            rayTracePassShader.SetInput(2, normalsBuffer);
            rayTracePassShader.SetInput(3, specularRoughnessBuffer);
            rayTracePassShader.SetOutput(rayTraceBuffer);
            rayTracePassShader.Draw(context, "Ray Trace");

            // Resolve Pass
            resolvePassShader.SetInput(0, blurPassBuffer);
            resolvePassShader.SetInput(1, ResolvePassResolution == ResolutionMode.Full ? depthBuffer : smallerDepthBuffer);
            resolvePassShader.SetInput(2, normalsBuffer);
            resolvePassShader.SetInput(3, specularRoughnessBuffer);
            resolvePassShader.SetInput(4, rayTraceBuffer);
            resolvePassShader.SetOutput(resolveBuffer);
            resolvePassShader.Draw(context, "Resolve");

            // Temporal Pass
            Texture reflectionsBuffer = resolveBuffer;
            if (TemporalEnabled)
            {
                var temporalSize = outputBuffer.Size;
                if (temporalBuffer == null || temporalBuffer.Size != temporalSize)
                {
                    temporalBuffer?.Dispose();
                    temporalBuffer = Texture.New2D(GraphicsDevice, temporalSize.Width, temporalSize.Height, 1, reflectionsFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                }

                Texture temporalBuffer0 = NewScopedRenderTarget2D(temporalSize.Width, temporalSize.Height, reflectionsFormat, 1);

                temporalPassShader.SetInput(0, resolveBuffer);
                temporalPassShader.SetInput(1, temporalBuffer);
                temporalPassShader.SetInput(2, depthBuffer);
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
