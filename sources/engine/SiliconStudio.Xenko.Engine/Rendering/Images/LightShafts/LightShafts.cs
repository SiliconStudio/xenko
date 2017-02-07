// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;
using DirectionalShaderData = SiliconStudio.Xenko.Rendering.Shadows.LightDirectionalShadowMapRenderer.ShaderData;

namespace SiliconStudio.Xenko.Rendering.Images
{
    [DataContract("LightShafts")]
    public class LightShafts : ImageEffect
    {
        private ImageEffectShader scatteringEffectShader;
        private ImageEffectShader applyLightEffectShader;
        private GaussianBlur blur;

        private IShadowMapRenderer shadowMapRenderer;
        private IEnumerable<LightShaftData> lightShaftDatas;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // Light accumulation shader
            scatteringEffectShader = ToLoadAndUnload(new ImageEffectShader("LightShaftsShader"));

            // Additive blending shader
            applyLightEffectShader = ToLoadAndUnload(new ImageEffectShader("AdditiveLightShader"));
            applyLightEffectShader.BlendState = new BlendStateDescription(Blend.One, Blend.One);

            blur = ToLoadAndUnload(new GaussianBlur());

            // Need the shadow map renderer in order to render light shafts
            var meshRenderFeature = Context.RenderSystem.RenderFeatures.OfType<MeshRenderFeature>().FirstOrDefault();
            if(meshRenderFeature == null)
                throw new ArgumentNullException("Missing mesh render feature");

            var forwardLightingFeature = meshRenderFeature.RenderFeatures.OfType<ForwardLightingRenderFeature>().FirstOrDefault();
            if (forwardLightingFeature == null)
                throw new ArgumentNullException("Missing forward lighting render feature");

            shadowMapRenderer = forwardLightingFeature.ShadowMapRenderer;
        }

        public void Collect(RenderContext context)
        {
            var processor = context.SceneInstance.GetProcessor<LightShaftProcessor>();
            if (processor == null)
            {
                lightShaftDatas = null;
                return;
            }

            lightShaftDatas = processor.LightShafts;
        }
        
        protected override void DrawCore(RenderDrawContext context)
        {
            if (lightShaftDatas == null)
                return; // Not collected

            var depthInput = GetSafeInput(0);

            int lightBufferDownsampleLevel = 1;
            var lightBuffer = NewScopedRenderTarget2D(depthInput.Width/lightBufferDownsampleLevel, depthInput.Height/lightBufferDownsampleLevel, PixelFormat.R16_Float);
            scatteringEffectShader.SetInput(0, depthInput); // Bind scene depth
            scatteringEffectShader.SetOutput(lightBuffer);

            if (!Initialized)
                Initialize(context.RenderContext);

            var renderView = context.RenderContext.RenderView;
            var viewInverse = Matrix.Invert(renderView.View);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.Eye, viewInverse.TranslationVector);

            var viewProjectionInverse = Matrix.Invert(renderView.ViewProjection);
            Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            center = Vector3.TransformCoordinate(center, viewProjectionInverse);
            right = Vector3.TransformCoordinate(right, viewProjectionInverse) - center;
            up = Vector3.TransformCoordinate(up, viewProjectionInverse) - center;

            // Basis for constructing world space rays originating from the camera
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ViewBase, center);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ViewRight, right);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ViewUp, up);

            // Used to project an arbitrary world space point into a linear depth value
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.CameraForward, viewInverse.Forward);

            // Used to convert values from the depth buffer to linear depth
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ZProjection, CameraKeys.ZProjectionACalculate(renderView.NearClipPlane, renderView.FarClipPlane));

            // Time % 1.0
            //imageEffectShader.Parameters.Set(LightShaftsShaderKeys.Time, (float)(context.RenderContext.Time.Elapsed.TotalSeconds % 1.0));
            // Time
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.Time, (float)(context.RenderContext.Time.Elapsed.TotalSeconds));

            foreach (var lightShaft in lightShaftDatas)
            {
                if (lightShaft.LightComponent == null)
                    continue; // Skip entities without a light component

                if (!shadowMapRenderer.ShadowMaps.TryGetValue(lightShaft.LightComponent, out lightShaft.ShadowMapTexture))
                    continue;

                if (lightShaft.ShadowMapTexture == null)
                    continue; // Skip lights without shadow map

                // Light accumulation pass (on low resolution buffer)
                DrawLightShaft(context, lightShaft);

                // Blur the result
                //blur.Radius = lightBufferDownsampleLevel;
                //blur.SetInput(lightBuffer);
                //blur.SetOutput(lightBuffer);
                //blur.Draw(context);

                // Additive blend pass
                Color3 lightColor = lightShaft.Light.Color.ComputeColor()*lightShaft.LightComponent.Intensity;
                applyLightEffectShader.Parameters.Set(AdditiveLightShaderKeys.LightColor, lightColor);
                applyLightEffectShader.SetInput(lightBuffer);
                applyLightEffectShader.SetOutput(GetSafeOutput(0));
                applyLightEffectShader.Draw(context);
            }
        }

        private void DrawLightShaft(RenderDrawContext context, LightShaftData lightShaft)
        {
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionFactor, lightShaft.ExtinctionFactor);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ExtinctionRatio, lightShaft.ExtinctionRatio);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.DensityFactor, lightShaft.DensityFactor);

            var shadowMapTexture = lightShaft.ShadowMapTexture.Atlas.Texture;

            // Bind shadow atlas
            scatteringEffectShader.Parameters.Set(ShadowMapKeys.Texture, lightShaft.ShadowMapTexture.Atlas.Texture);

            var shadowMapTextureSize = new Vector2(shadowMapTexture.Width, shadowMapTexture.Height);
            var shadowMapTextureTexelSize = 1.0f/shadowMapTextureSize;
            scatteringEffectShader.Parameters.Set(ShadowMapKeys.TextureSize, shadowMapTextureSize);
            scatteringEffectShader.Parameters.Set(ShadowMapKeys.TextureTexelSize, shadowMapTextureTexelSize);

            // Pass in world transform as offset and direction
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowLightOffset, lightShaft.LightWorld.TranslationVector);
            scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowLightDirection, lightShaft.LightWorld.Forward);

            // Change inputs depending on light type
            if (lightShaft.ShadowMapTexture.ShaderData is DirectionalShaderData)
            {
                var light = (LightDirectional)lightShaft.Light;
                var shaderData = (DirectionalShaderData)lightShaft.ShadowMapTexture.ShaderData;
                var shadowRectangle = lightShaft.ShadowMapTexture.GetRectangle(0);

                Vector4 shadowBounds = new Vector4(
                    shadowRectangle.Left*shadowMapTextureTexelSize.X, shadowRectangle.Top*shadowMapTextureTexelSize.Y,
                    shadowRectangle.Right*shadowMapTextureTexelSize.X, shadowRectangle.Bottom*shadowMapTextureTexelSize.Y);
                scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowBounds, shadowBounds);

                // Use cascade 0 of directional light
                scatteringEffectShader.Parameters.Set(LightShaftsShaderKeys.ShadowViewProjection, shaderData.WorldToShadowCascadeUV[0]);
            }

            scatteringEffectShader.Draw(context, $"Light Shafts [{lightShaft.LightComponent.Entity.Name}]");
        }
    }
}
