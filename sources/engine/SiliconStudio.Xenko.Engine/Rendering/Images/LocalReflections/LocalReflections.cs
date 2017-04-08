// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using System.Diagnostics;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Compute screen space reflections as a post effect.
    /// </summary>
    [DataContract("LocalReflections")]
    public sealed class LocalReflections : ImageEffect
    {
        ImageEffectShader rayTracePassShader;

        // TOOD: cleanup this! turn into local variables
        Vector2 screenSize;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix viewProjectionMatrix;
        Vector4 eye;
        float nearclip;
        float farclip;
        Matrix inverseViewMatrix;
        Matrix inverseViewProjectionMatrix;
        float aspect;
        float fieldOfView;
        Vector4 viewDirection;
        Vector4 viewDirectionOrtho;
        Vector4 zPlanes;

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
        /*
        // TOOD: cleanup this! remove unused params
        /// <summary>
        /// maximum number of dynamic iterations in the HiZ trace
        /// </summary>
        [Display("Max num steps")]
        [DefaultValue(45)]
        public int MaxNumSteps { get; set; } = 45;

        /// <summary>
        /// ray tracing starting position is offseted by a percent of the normal in world space to avoid self occlusions.
        /// </summary>
        [Display("Ray start bias")]
        [DefaultValue(0.03f)]
        public float WorldAntiSelfOcclusionBias { get; set; } = 0.03f;

        /// <summary>
        /// this represents the thickness of the depth buffer surface
        /// </summary>
        [Display("Pixel depth")]
        [DefaultValue(0.01f)]
        public float PixelDepth { get; set; } = 0.01f;

        [Display("Max depth")]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        [DefaultValue(1.0f)]
        public float MaxTravelDistance { get; set; } = 1.0f;
        */
        protected override void InitializeCore()
        {
            base.InitializeCore();

            rayTracePassShader = ToLoadAndUnload(new ImageEffectShader("SSLRRayTracePass"));
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
        /// <param name="normalsBuffer">The buffer which contains surface normals and roughness.</param>
        public void SetInputSurfaces(Texture colorBuffer, Texture depthBuffer, Texture normalsBuffer)
        {
            SetInput(0, colorBuffer);
            SetInput(1, depthBuffer);
            SetInput(2, normalsBuffer);
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            if (!rayTracePassShader.Initialized)
                rayTracePassShader.Initialize(context.RenderContext);

            // inputs:
            /*var depthBuffer = GetSafeInput(1);
            // output:
            var outputTarget = GetSafeOutput(0);

            VerifyDirty(depthBuffer);
            if (configurationDirty)
                SetupTechnique(context.RenderContext, depthBuffer, outputTarget);

            screenSize = new Vector2();
            var currentCamera = context.RenderContext.GetCurrentCamera();
            if (currentCamera == null)
                throw new InvalidOperationException("No valid camera");
            viewMatrix = currentCamera.ViewMatrix;
            projectionMatrix = currentCamera.ProjectionMatrix;
            viewProjectionMatrix = currentCamera.ViewProjectionMatrix;
            inverseViewMatrix = Matrix.Invert(viewMatrix);
            eye = inverseViewMatrix.Row4;
            nearclip = currentCamera.NearClipPlane;
            farclip = currentCamera.FarClipPlane;
            inverseViewProjectionMatrix = Matrix.Invert(viewProjectionMatrix);
            aspect = currentCamera.AspectRatio;
            fieldOfView = (float)(2.0f * Math.Atan2(projectionMatrix.M11, aspect));

            viewDirection = new Vector4(0, 0, -1, 0);
            viewDirection = Vector4.Transform(viewDirection, inverseViewMatrix);

            viewDirectionOrtho = new Vector4(1, 0, 0, 0);
            viewDirectionOrtho = Vector4.Transform(viewDirectionOrtho, inverseViewMatrix);

            zPlanes = new Vector4(nearclip, farclip, 0, fieldOfView);

            var resolutionAsSize2 = GetReflectionBufferResolution(outputTarget);
            screenSize.X = resolutionAsSize2.Width;
            screenSize.Y = resolutionAsSize2.Height;

            // uniform settings:
            localReflectionShader.Parameters.Set(SSLRShaderKeys.ScreenSize, screenSize);
            localReflectionShader.Parameters.Set(SSLRCommonKeys.ZPlanes, zPlanes);
            localReflectionShader.Parameters.Set(SSLRCommonKeys.ViewProjectionMatrix, viewProjectionMatrix);
            localReflectionShader.Parameters.Set(SSLRCommonKeys.InverseViewProjectionMatrix, inverseViewProjectionMatrix);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.CameraPosWS, eye);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.ViewDirection, viewDirection);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.ViewDirectionOrthoLeft, viewDirectionOrtho);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.MaxNumSteps, MaxNumSteps);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.WorldAntiSelfOcclusionBias, WorldAntiSelfOcclusionBias);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.PixelDepth, PixelDepth);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.MaxHizMipLevel, hizChain.MipLevels - 1);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.MaxTravelDistance, MaxTravelDistance);
            localReflectionShader.Parameters.Set(SSLRShaderKeys.ResolutionDivisor, GetResolutionDivisor());

            roughnessBlurAndReflectionCompositing.Parameters.Set(SSLRCommonKeys.InverseViewProjectionMatrix, inverseViewProjectionMatrix);
            roughnessBlurAndReflectionCompositing.Parameters.Set(SSLRCommonKeys.ZPlanes, zPlanes);
            roughnessBlurAndReflectionCompositing.Parameters.Set(ReflectionBRDFCompositingKeys.CameraPosWS, eye);*/
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Inputs:
            Texture colorBuffer = GetSafeInput(0);
            Texture depthBuffer = GetSafeInput(1);
            Texture normalsBuffer = GetSafeInput(2);

            // Output:
            Texture outputBuffer = GetSafeOutput(0);

            // Get temporary buffers (use small formats, we don't want to kill performance)
            var traceBuffersSize = GetTraceBufferResolution(depthBuffer);
            Texture rayTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R16G16_Float, 1);
            Texture coneTraceBuffer = NewScopedRenderTarget2D(traceBuffersSize.Width, traceBuffersSize.Height, PixelFormat.R8G8B8A8_UNorm, 1);

            // TODO: Blur Pass

            // TODO: Ray Trace Pass

            // TODO: Cone Trace Pass

            // TODO: Combine Pass

            // temporary clear output
            context.CommandList.Clear(outputBuffer, Color.BlueViolet);

            /*localReflectionShader.SetInput(0, colorBuffer);
            localReflectionShader.SetInput(1, hizChainSkipMip);
            localReflectionShader.SetInput(2, gBuffer);
            localReflectionShader.SetInput(3, gBuffer2);
            localReflectionShader.SetOutput(reflectionBuffer);
            localReflectionShader.Draw(context, "trace reflections");

            Texture tmpOutput = NewScopedRenderTarget2D(reflectionBuffer.Width, reflectionBuffer.Height, reflectionBuffer.Format);

            // H-V blur pass here for roughness.
            hRoughBlurPass.SetInput(0, reflectionBuffer);
            hRoughBlurPass.SetInput(1, hizChainSkipMip);
            hRoughBlurPass.SetInput(2, gBuffer);
            hRoughBlurPass.SetInput(3, gBuffer2);
            hRoughBlurPass.SetOutput(tmpOutput);
            hRoughBlurPass.Draw(context, "hblur rough");

            vRoughBlurPass.SetInput(0, tmpOutput);
            vRoughBlurPass.SetInput(1, hizChainSkipMip);
            vRoughBlurPass.SetInput(2, gBuffer);
            vRoughBlurPass.SetInput(3, gBuffer2);
            vRoughBlurPass.SetOutput(reflectionBuffer);
            vRoughBlurPass.Draw(context, "vblur rough");

            roughnessBlurAndReflectionCompositing.SetInput(0, reflectionBuffer);  // RGB: ray traced color pick | A: ray length
            roughnessBlurAndReflectionCompositing.SetInput(1, colorBuffer);       // scene color (with IBL)
            roughnessBlurAndReflectionCompositing.SetInput(2, hizChain);          // depth chain
            roughnessBlurAndReflectionCompositing.SetInput(3, gBuffer);           // RG:  normals | BA: spec color
            roughnessBlurAndReflectionCompositing.SetInput(4, gBuffer2);          // RGB: IBL     | A:  packed( material roughness, material reflectivity )
            roughnessBlurAndReflectionCompositing.SetOutput(outputTarget);
            roughnessBlurAndReflectionCompositing.Draw(context, "IBL mix & fresnel");*/
        }
    }
}