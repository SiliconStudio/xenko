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

        protected override void InitializeCore()
        {
            base.InitializeCore();

            blurPassShader = ToLoadAndUnload(new ImageEffectShader("SSLRBlurPassEffect"));
            rayTracePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRRayTracePass"));
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
            Vector4 ZPlanes = new Vector4(nearclip, farclip, 0, fieldOfView); // x = Frustum Near, y = Frustum Far, w = FOV

            var traceBufferSize = GetTraceBufferResolution(outputBuffer);
            var roughnessFade = MathUtil.Clamp(MaxRoughness, 0.0f, 1.0f);
            var maxTraceSamples = MathUtil.Clamp(MaxStepsAmount, 1, 128);

            // ViewInfo    :  x-1/Projection[0,0]   y-1/Projection[1,1]   z-(Far / (Far - Near)   w-(-Far * Near) / (Far - Near) / Far)
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ViewInfo, new Vector4(1.0f / projectionMatrix.M11, 1.0f / projectionMatrix.M22, farclip / (farclip - nearclip), (-farclip * nearclip) / (farclip - nearclip) / farclip));
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ViewFarPlane, farclip);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.RoughnessFade, roughnessFade);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.MaxTraceSamples, maxTraceSamples);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.CameraPosWS, eye);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ZPlanes, ZPlanes);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.ScreenSize, new Vector2(traceBufferSize.Width, traceBufferSize.Height));
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.RayStepScale, 2.0f / outputBuffer.Width);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.V, viewMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.P, projectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.VP, viewProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.IV, inverseViewMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.IP, inverseProjectionMatrix);
            rayTracePassShader.Parameters.Set(SSLRCommonKeys.IVP, inverseViewProjectionMatrix);
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
            var traceBuffersSize = GetTraceBufferResolution(outputBuffer);
            var colorBuffersSize = new Size2(outputBuffer.Width / 2, outputBuffer.Height / 2);
            Texture rayTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R8G8_UNorm, 1);
            Texture coneTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R8G8B8A8_UNorm, 1);
            Texture colorBuffer0 = NewScopedRenderTarget2D(colorBuffersSize.Width, colorBuffersSize.Height, PixelFormat.R11G11B10_Float, MipMapCount.Auto);
            Texture colorBuffer1 = NewScopedRenderTarget2D(colorBuffersSize.Width, colorBuffersSize.Height, PixelFormat.R11G11B10_Float, MipMapCount.Auto);

            // Cache per colro buffer mip views
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

            // Blur Pass
            for (int mipLevel = 0; mipLevel < colorMipLevels; mipLevel++)
            {
                int mipWidth = colorBuffer0.Width >> mipLevel;
                int mipHeight = colorBuffer1.Height >> mipLevel;

                blurPassShader.Parameters.Set(SSLRCommonKeys.TexelSize, new Vector2(1.0f / mipWidth, 1.0f / mipHeight));

                // Blur H
                var srcMip = mipLevel == 0 ? colorBuffer : cachedColorBuffer0Mips[mipLevel - 1];
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
            
            // TODO: Cone Trace Pass

            // TODO: Combine Pass
            
            //context.CommandList.Clear(outputBuffer, Color.BlueViolet);
            Scaler.SetInput(0, rayTraceBuffer);
            //Scaler.SetInput(0, cachedColorBuffer0Mips[3]);
            Scaler.SetOutput(outputBuffer);
            Scaler.Draw(context);
        }
    }
}