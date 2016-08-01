// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Applies an ambient occlusion effect to a scene. Ambient occlusion is a technique which fakes occlusion for objects close to other opaque objects.
    /// It takes as input a color-buffer where the scene was rendered, with its associated depth-buffer.
    /// You also need to provide the camera configuration you used when rendering the scene.
    /// </summary>
    [DataContract("AmbientOcclusion")]
    public class AmbientOcclusion : ImageEffect
    {
        private ImageEffectShader aoImageEffect;
        private ImageEffectShader cszImageEffect;
        private ImageEffectShader blurH;
        private ImageEffectShader blurV;
        private string nameGaussianBlurH;
        private string nameGaussianBlurV;

        private Vector2[] offsetsWeights;

        [DataMember(10)]
        [DefaultValue(9)]
        [DataMemberRange(1, 100)]
        [Display("Number of samples")]
        public int NumberOfSamples { get; set; } = 9;



        [DataMember(20)]
        [DefaultValue(1)]
        [Display("Projection Scale")]
        public float ParamProjScale { get; set; } = 1f;

        [DataMember(30)]
        [DefaultValue(1)]
        [Display("Occlusion Intensity")]
        public float ParamIntensity { get; set; } = 1f;

        [DataMember(40)]
        [DefaultValue(0.01f)]
        [Display("Bias")]
        public float ParamBias { get; set; } = 0.01f;

        [DataMember(50)]
        [DefaultValue(1)]
        [Display("Tap Radius")]
        public float ParamRadius { get; set; } = 1f;



        public AmbientOcclusion()
        {
            //Enabled = false;
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            aoImageEffect  = ToLoadAndUnload(new ImageEffectShader("ApplyAmbientOcclusionShader"));
            cszImageEffect = ToLoadAndUnload(new ImageEffectShader("ReconstructCameraSpaceZ"));

            blurH = ToLoadAndUnload(new ImageEffectShader("GaussianBlurEffect"));
            blurV = ToLoadAndUnload(new ImageEffectShader("GaussianBlurEffect"));
            blurH.Initialize(Context);
            blurV.Initialize(Context);

            // Setup Horizontal parameters
            blurH.Parameters.Set(GaussianBlurKeys.VerticalBlur, false);
            blurV.Parameters.Set(GaussianBlurKeys.VerticalBlur, true);
        }

        protected override void Destroy()
        {
            base.Destroy();
        }

        /// <summary>
        /// Provides a color buffer and a depth buffer to apply the depth-of-field to.
        /// </summary>
        /// <param name="colorBuffer">A color buffer to process.</param>
        /// <param name="depthBuffer">The depth buffer corresponding to the color buffer provided.</param>
        public void SetColorDepthInput(Texture colorBuffer, Texture depthBuffer)
        {
            SetInput(0, colorBuffer);
            SetInput(1, depthBuffer);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var originalColorBuffer = GetSafeInput(0);
            var originalDepthBuffer = GetSafeInput(1);

            var outputTexture = GetSafeOutput(0);

            var camera = context.RenderContext.GetCurrentCamera();

            //---------------------------------
            // Ambient Occlusion
            //---------------------------------

            var aoTexture1 = NewScopedRenderTarget2D(originalColorBuffer.Width, originalColorBuffer.Height, PixelFormat.R8_UNorm, 1);
            var aoTexture2 = NewScopedRenderTarget2D(originalColorBuffer.Width, originalColorBuffer.Height, PixelFormat.R8_UNorm, 1);

            if (camera != null)
            {
                // Set Near/Far pre-calculated factors to speed up the linear depth reconstruction
                cszImageEffect.Parameters.Set(CameraKeys.ZProjection, CameraKeys.ZProjectionACalculate(camera.NearClipPlane, camera.FarClipPlane));

                Vector4 ScreenSize = new Vector4(originalColorBuffer.Width, originalColorBuffer.Height, 0, 0);
                ScreenSize.Z = ScreenSize.X / ScreenSize.Y;
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ScreenInfo, ScreenSize);

                // Projection infor used to reconstruct the View space position from linear depth
                var p00 = camera.ProjectionMatrix.M11;
                var p11 = camera.ProjectionMatrix.M22;
                var p02 = camera.ProjectionMatrix.M13;
                var p12 = camera.ProjectionMatrix.M23;
                Vector4 projInfo = new Vector4(-2.0f / (ScreenSize.X * p00), -2.0f / (ScreenSize.Y * p11), (1.0f - p02) / p00, (1.0f + p12) / p11);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ProjInfo, projInfo);

                //**********************************
                // User parameters
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ParamProjScale, ParamProjScale);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ParamIntensity, ParamIntensity);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ParamBias, ParamBias);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ParamRadius, ParamRadius);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ParamRadiusSquared, ParamRadius * ParamRadius);
            }

            cszImageEffect.SetInput(0, originalColorBuffer);
            cszImageEffect.SetInput(1, originalDepthBuffer);
            cszImageEffect.SetOutput(aoTexture1);
            cszImageEffect.Draw(context, "CameraSpaceZAO");


            int Radius = 5;
            float SigmaRatio = 2.0f;
            var size = Radius * 2 + 1;
            if (offsetsWeights == null)
            {
                nameGaussianBlurH = string.Format("GaussianBlurH{0}x{0}", size);
                nameGaussianBlurV = string.Format("GaussianBlurV{0}x{0}", size);

                // TODO: cache if necessary
                offsetsWeights = GaussianUtil.Calculate1D(Radius, SigmaRatio);
            }

            // Update permutation parameters
            blurH.Parameters.Set(GaussianBlurKeys.Count, offsetsWeights.Length);
            blurV.Parameters.Set(GaussianBlurKeys.Count, offsetsWeights.Length);
            blurH.EffectInstance.UpdateEffect(context.GraphicsDevice);
            blurV.EffectInstance.UpdateEffect(context.GraphicsDevice);

            // Update parameters
            blurH.Parameters.Set(GaussianBlurShaderKeys.OffsetsWeights, offsetsWeights);
            blurV.Parameters.Set(GaussianBlurShaderKeys.OffsetsWeights, offsetsWeights);

            // Horizontal pass
            blurH.SetInput(aoTexture1);
            blurH.SetOutput(aoTexture2);
            blurH.Draw(context, nameGaussianBlurH);

            // Vertical pass
            blurV.SetInput(aoTexture2);
            blurV.SetOutput(aoTexture1);
            blurV.Draw(context, nameGaussianBlurV);



            aoImageEffect.SetInput(0, originalColorBuffer);
            aoImageEffect.SetInput(1, aoTexture1);
            aoImageEffect.SetOutput(outputTexture);
            aoImageEffect.Draw(context, "AmbientOcclusion");


        }
    }
}
