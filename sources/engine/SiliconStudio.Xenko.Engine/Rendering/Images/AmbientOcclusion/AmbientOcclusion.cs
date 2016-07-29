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

        [DataMember(10)]
        [DefaultValue(15)]
        [DataMemberRange(1, 100)]
        [Display("Number of samples")]
        public int NumberOfSamples { get; set; }

        public AmbientOcclusion()
        {
            //Enabled = false;
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            aoImageEffect = ToLoadAndUnload(new ImageEffectShader("ScreenSpaceAmbientOcclusion"));
            cszImageEffect = ToLoadAndUnload(new ImageEffectShader("ReconstructCameraSpaceZ"));
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
            // Camera Space Z
            //---------------------------------
            if (camera != null)
            {
                cszImageEffect.Parameters.Set(CameraKeys.ZProjection, CameraKeys.ZProjectionACalculate(camera.NearClipPlane, camera.FarClipPlane));

                Matrix projInverse;
                Matrix.Invert(ref camera.ProjectionMatrix, out projInverse);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.InverseProjection, projInverse);

                Matrix viewInverse;
                Matrix.Invert(ref camera.ViewMatrix, out viewInverse);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.InverseView, viewInverse);

                Matrix viewProj = camera.ViewMatrix * camera.ProjectionMatrix;
                Matrix viewProjInverse;
                Matrix.Invert(ref viewProj, out viewProjInverse);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.InverseViewProjection, viewProjInverse);

                // DON'T NEED THIS
                Vector4 clipInfo = new Vector4(100f, -999.9f, 1000f, 0);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ClipInfo, clipInfo);

                var p00 = camera.ProjectionMatrix.M11;
                var p11 = camera.ProjectionMatrix.M22;
                var p02 = camera.ProjectionMatrix.M13;
                var p12 = camera.ProjectionMatrix.M23;
                Vector4 projInfo = new Vector4(-2.0f / (1024 * p00), -2.0f / (768 * p11), (1.0f - p02) / p00, (1.0f + p12) / p11);
                cszImageEffect.Parameters.Set(ReconstructCameraSpaceZKeys.ProjInfo, projInfo);

            }
            cszImageEffect.SetInput(0, originalColorBuffer);
            cszImageEffect.SetInput(1, originalDepthBuffer);
            cszImageEffect.SetOutput(outputTexture);
            cszImageEffect.Draw(context, "CameraSpaceZ");

            //---------------------------------
            // Ambient Occlusion
            //---------------------------------

            //aoImageEffect.Parameters.Set(ThresholdAlphaCoCKeys.CoCReference, previousCoC);
            //aoImageEffect.Parameters.Set(ThresholdAlphaCoCKeys.CoCCurrent, levelConfig.CoCValue);

            /*
                aoImageEffect.SetInput(0, originalColorBuffer);
                aoImageEffect.SetInput(1, originalDepthBuffer);
                aoImageEffect.SetOutput(outputTexture);
                aoImageEffect.Draw(context, "AmbientOcclusion");
            */
        }
    }
}
