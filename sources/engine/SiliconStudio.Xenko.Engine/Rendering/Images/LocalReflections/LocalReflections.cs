#define SSLR_DEBUG

// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
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
        private ImageEffectShader blurPassShader;
        private ImageEffectShader rayTracePassShader;
        private ImageEffectShader coneTracePassShader;
        private ImageEffectShader combinePassShader;

        private Texture[] cachedColorBuffer0Mips;
        private Texture[] cachedColorBuffer1Mips;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalReflections"/> class.
        /// </summary>
        public LocalReflections()
        {
            ResolutionDivisor = ResolutionDivisors.Half;
        }

        [DataContract("ResolutionDivisors")]
        public enum ResolutionDivisors
        {
            /// <summary>
            /// Use a small size.
            /// </summary>
            /// <userodc>1/4 resolution</userodc>
            [Display("/ 4")]
            Quarter,

            /// <summary>
            /// Use a medium size.
            /// </summary>
            /// <userodc>1/2 resolution</userodc>
            [Display("/ 2")]
            Half,

            /// <summary>
            /// Use a large size.
            /// </summary>
            /// <userodc>x 1 resolution</userodc>
            [Display("x 1")]
            Full
        }

        [Display("Resolution divisor")]
        [DefaultValue(ResolutionDivisors.Half)]
        public ResolutionDivisors ResolutionDivisor { get; set; }

        /// <summary>
        /// Maximum allowed amount of dynamic iterations in the ray trace pass.
        /// </summary>
        [Display("Max steps amount")]
        [DefaultValue(24)]
        public int MaxStepsAmount { get; set; } = 24;

        /// <summary>
        /// Maximum allowed surface roughness value to use local reflections.
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

#if SSLR_DEBUG

        public enum DebugModes
        {
            None,
            RayCast,
            ConeTrace,
        }

        public DebugModes DebugMode = DebugModes.None;

#endif

        protected override void InitializeCore()
        {
            base.InitializeCore();
            
            blurPassShader = ToLoadAndUnload(new ImageEffectShader("SSLRBlurPassEffect"));
            rayTracePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRRayTracePass"));
            coneTracePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRConeTracePass"));
            combinePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRCombinePass"));
        }

        protected override void Destroy()
        {
            cachedColorBuffer0Mips?.ForEach(view => view?.Dispose());
            cachedColorBuffer1Mips?.ForEach(view => view?.Dispose());

            base.Destroy();
        }

        private int GetResolutionDivisor()
        {
            int divisor = 1;
            switch (ResolutionDivisor)
            {
                case ResolutionDivisors.Full:
                    divisor = 1;
                    break;
                case ResolutionDivisors.Half:
                    divisor = 2;
                    break;
                case ResolutionDivisors.Quarter:
                    divisor = 4;
                    break;
            }
            return divisor;
        }

        private Size3 GetTraceBufferResolution(Texture fullResTarget)
        {
            var divisor = GetResolutionDivisor();
            return new Size3(fullResTarget.Width / divisor, fullResTarget.Height / divisor, 1);
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
            if (!coneTracePassShader.Initialized)
                coneTracePassShader.Initialize(context.RenderContext);
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

            var traceBufferSize = GetTraceBufferResolution(outputBuffer);
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
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);

            // TODO: check which keys are used by coneTracePassShader

            coneTracePassShader.Parameters.Set(SSLRCommonKeys.MaxColorMiplevel, Texture.CalculateMipMapCount(0, outputBuffer.Width, outputBuffer.Height) - 1);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.TraceSizeMax, Math.Max(traceBufferSize.Width, traceBufferSize.Height) / 2.0f);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.SSRtexelSize, new Vector2(1.0f / traceBufferSize.Width, 1.0f / traceBufferSize.Height));
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, ViewInfo);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.MaxTraceSamples, maxTraceSamples);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, eye);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.ScreenSize, ScreenSize);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.RayStepScale, 2.0f / outputBuffer.Width);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            coneTracePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);

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
            combinePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            combinePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            combinePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);
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

            // Get temporary buffers (use small formats, we don't want to kill performance)
            // Note: we convole color buffer into half size because it's super fast
            var traceBuffersSize = GetTraceBufferResolution(outputBuffer);
            var colorBuffersSize = new Size2(outputBuffer.Width / 2, outputBuffer.Height / 2);
#if SSLR_DEBUG
            Texture rayTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R32G32B32A32_Float, 1);
#else
            Texture rayTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R8G8_UNorm, 1);
#endif
            Texture coneTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R11G11B10_Float, 1);
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

            // Blur Pass
            for (int mipLevel = 1; mipLevel < colorMipLevels; mipLevel++)
            {
                int mipWidth = colorBuffer0.Width >> mipLevel;
                int mipHeight = colorBuffer1.Height >> mipLevel;

                blurPassShader.Parameters.Set(SSLRCommonKeys.TexelSize, new Vector2(1.0f / mipWidth, 1.0f / mipHeight));

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

            // Ray Trace Pass
            rayTracePassShader.SetInput(0, colorBuffer);
            rayTracePassShader.SetInput(1, depthBuffer);
            rayTracePassShader.SetInput(2, normalsBuffer);
            rayTracePassShader.SetInput(3, specularRoughnessBuffer);
            rayTracePassShader.SetOutput(rayTraceBuffer);
            rayTracePassShader.Draw(context, "Ray Trace");

            // Cone Trace Pass
            coneTracePassShader.SetInput(0, colorBuffer0);
            coneTracePassShader.SetInput(1, depthBuffer);
            coneTracePassShader.SetInput(2, normalsBuffer);
            coneTracePassShader.SetInput(3, specularRoughnessBuffer);
            coneTracePassShader.SetInput(4, rayTraceBuffer);
            coneTracePassShader.SetOutput(coneTraceBuffer);
            coneTracePassShader.Draw(context, "Cone Trace");

            // Combine Pass
            combinePassShader.SetInput(0, colorBuffer);
            combinePassShader.SetInput(1, depthBuffer);
            combinePassShader.SetInput(2, normalsBuffer);
            combinePassShader.SetInput(3, specularRoughnessBuffer);
            combinePassShader.SetInput(4, coneTraceBuffer);
            combinePassShader.SetOutput(outputBuffer);
            combinePassShader.Draw(context, "Combine");

#if SSLR_DEBUG
            if (DebugMode != DebugModes.None)
            {
                // Debug preview of temp targets
                switch (DebugMode)
                {
                    case DebugModes.RayCast:
                        Scaler.SetInput(0, rayTraceBuffer);
                        break;
                    case DebugModes.ConeTrace:
                        Scaler.SetInput(0, coneTraceBuffer);
                        break;
                }
                Scaler.SetOutput(outputBuffer);
                Scaler.Draw(context);
            }
#endif
        }
    }
}