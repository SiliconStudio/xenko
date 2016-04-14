// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Light renderer for <see cref="LightSkybox"/>.
    /// </summary>
    public class LightSkyboxRenderer : LightGroupRendererBase
    {
        public LightSkyboxRenderer()
        {
            IsEnvironmentLight = true;
            LightMaxCount = 4;
        }
       
        public override LightShaderGroup CreateLightShaderGroup(string compositionName, int lightMaxCount, ILightShadowMapShaderGroupData shadowGroup)
        {
            var mixin = new ShaderMixinGeneratorSource("LightSkyboxEffect");
            return new LightSkyBoxShaderGroup(mixin, compositionName);
        }

        private class LightSkyBoxShaderGroup : LightShaderGroupAndDataPool<LightSkyBoxShaderGroupData>
        {
            internal readonly ValueParameterKey<float> IntensityKey;
            internal readonly ValueParameterKey<Matrix> SkyMatrixKey;
            internal readonly PermutationParameterKey<ShaderSource> LightDiffuseColorKey;
            internal readonly PermutationParameterKey<ShaderSource> LightSpecularColorKey;
            internal readonly ValueParameterKey<Color3> SphericalColorsKey;
            internal readonly ObjectParameterKey<Texture> SpecularCubeMapkey;
            internal readonly ValueParameterKey<float> SpecularMipCountKey;

            public LightSkyBoxShaderGroup(ShaderSource mixin, string compositionName)
                : base(mixin, compositionName, null)
            {
                IntensityKey = LightSkyboxShaderKeys.Intensity.ComposeWith(compositionName);
                SkyMatrixKey = LightSkyboxShaderKeys.SkyMatrix.ComposeWith(compositionName);
                LightDiffuseColorKey = LightSkyboxShaderKeys.LightDiffuseColor.ComposeWith(compositionName);
                LightSpecularColorKey = LightSkyboxShaderKeys.LightSpecularColor.ComposeWith(compositionName);

                SphericalColorsKey = SphericalHarmonicsEnvironmentColorKeys.SphericalColors.ComposeWith("lightDiffuseColor." + compositionName);
                SpecularCubeMapkey = RoughnessCubeMapEnvironmentColorKeys.CubeMap.ComposeWith("lightSpecularColor." + compositionName);
                SpecularMipCountKey = RoughnessCubeMapEnvironmentColorKeys.MipCount.ComposeWith("lightSpecularColor." + compositionName);
            }

            protected override LightSkyBoxShaderGroupData CreateData()
            {
                return new LightSkyBoxShaderGroupData(this);
            }
        }

        private class LightSkyBoxShaderGroupData : LightShaderGroupData
        {
            private readonly ValueParameterKey<float> intensityKey;

            private readonly ValueParameterKey<Matrix> skyMatrixKey;

            private readonly PermutationParameterKey<ShaderSource> lightDiffuseColorKey;

            private readonly PermutationParameterKey<ShaderSource> lightSpecularColorKey;

            private float intensity;

            private Matrix rotationMatrix;

            private ShaderSource lightDiffuseColorShader;

            private ShaderSource lightSpecularColorShader;

            private Color3[] sphericalColors;

            private readonly ValueParameterKey<Color3> sphericalColorsKey;

            private readonly ObjectParameterKey<Texture> specularCubeMapkey;

            private readonly ValueParameterKey<float> specularMipCountKey;

            private static readonly ShaderClassSource EmptyComputeEnvironmentColorSource = new ShaderClassSource("IComputeEnvironmentColor");

            private Skybox previousSkybox;

            private Texture specularCubemap;

            private int specularCubemapLevels;

            public LightSkyBoxShaderGroupData(LightSkyBoxShaderGroup group) : base(null)
            {
                intensityKey = group.IntensityKey;
                skyMatrixKey = group.SkyMatrixKey;
                lightDiffuseColorKey = group.LightDiffuseColorKey;
                lightSpecularColorKey = group.LightSpecularColorKey;

                sphericalColorsKey = group.SphericalColorsKey;
                specularCubeMapkey = group.SpecularCubeMapkey;
                specularMipCountKey = group.SpecularMipCountKey;
            }

            protected override void AddLightInternal(LightComponent light)
            {
                // TODO: If there is a performance penalty for accessing the SkyboxComponent, this could be prepared by the LightProcessor
                var lightSkybox = ((LightSkybox)light.Type);
                var skyboxComponent = lightSkybox.SkyboxComponent;
                var skybox = skyboxComponent.Skybox;

                intensity = light.Intensity;
                if (skyboxComponent.Enabled)
                {
                    intensity *= skyboxComponent.Intensity;
                }

                rotationMatrix = lightSkybox.SkyMatrix;

                var diffuseParameters = skybox.DiffuseLightingParameters;
                var specularParameters = skybox.SpecularLightingParameters;

                specularCubemap = specularParameters.Get(SkyboxKeys.CubeMap);
                if (specularCubemap != null)
                {
                    specularCubemapLevels = specularCubemap.MipLevels;
                }
                sphericalColors = diffuseParameters.GetValues(SphericalHarmonicsEnvironmentColorKeys.SphericalColors);
                lightDiffuseColorShader = diffuseParameters.Get(SkyboxKeys.Shader) ?? EmptyComputeEnvironmentColorSource;
                lightSpecularColorShader = specularParameters.Get(SkyboxKeys.Shader) ?? EmptyComputeEnvironmentColorSource;

                previousSkybox = skybox;
            }

            public override void ApplyEffectPermutations(RenderEffect renderEffect)
            {
                renderEffect.EffectValidator.ValidateParameter(lightDiffuseColorKey, lightDiffuseColorShader);
                renderEffect.EffectValidator.ValidateParameter(lightSpecularColorKey, lightSpecularColorShader);
            }

            protected override void ApplyParametersInternal(RenderDrawContext context, ParameterCollection parameters)
            {
                // global parameters
                parameters.Set(intensityKey, intensity);
                parameters.Set(skyMatrixKey, rotationMatrix);

                // This need to be working with new system
                parameters.Set(sphericalColorsKey, sphericalColors);
                parameters.Set(specularCubeMapkey, specularCubemap);
                parameters.Set(specularMipCountKey, specularCubemapLevels);
            }
        }
    }
}
