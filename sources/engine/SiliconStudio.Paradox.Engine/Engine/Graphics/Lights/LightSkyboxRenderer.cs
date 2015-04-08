// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Shadows;
using SiliconStudio.Paradox.Engine.Graphics.Skyboxes;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Light renderer for <see cref="LightSkybox"/>.
    /// </summary>
    public class LightSkyboxRenderer : LightGroupRendererBase
    {
        public LightSkyboxRenderer()
        {
            IsEnvironmentLight = true;
        }
       
        public override LightShaderGroup CreateLightShaderGroup(string compositionName, int compositionIndex, int lightMaxCount, ILightShadowMapShaderGroupData shadowGroup)
        {
            var mixin = new ShaderMixinGeneratorSource("LightSkyboxEffect");
            return new LightSkyBoxShaderGroup(compositionName, compositionIndex, mixin);
        }

        private class LightSkyBoxShaderGroup : LightShaderGroup
        {
            private readonly ParameterKey<float> intensityKey;

            private readonly ParameterKey<Matrix> skyMatrixKey;

            private readonly ParameterKey<ShaderSource> lightDiffuseColorKey;

            private readonly ParameterKey<ShaderSource> lightSpecularColorKey;

            private float intensity;

            private Matrix rotationMatrix;

            private ShaderSource lightDiffuseColorShader;

            private ShaderSource lightSpecularColorShader;

            private Color3[] sphericalColors;

            private readonly ParameterKey<Color3[]> sphericalColorsKey;

            private readonly ParameterKey<Texture> specularCubeMapkey;

            private readonly ParameterKey<float> specularMipCountKey;

            private static readonly ShaderClassSource EmptyComputeEnvironmentColorSource = new ShaderClassSource("IComputeEnvironmentColor");

            private Skybox previousSkybox;

            private Texture specularCubemap;

            public LightSkyBoxShaderGroup(string compositionName, int compositionIndex, ShaderSource mixin)
                : base(mixin, null)
            {
                intensityKey = LightSkyboxShaderKeys.Intensity.ComposeIndexer(compositionName, compositionIndex);
                skyMatrixKey = LightSkyboxShaderKeys.SkyMatrix.ComposeIndexer(compositionName, compositionIndex);
                lightDiffuseColorKey = LightSkyboxShaderKeys.LightDiffuseColor.ComposeIndexer("lightDiffuseColor." + compositionName, compositionIndex);
                lightSpecularColorKey = LightSkyboxShaderKeys.LightSpecularColor.ComposeIndexer("lightSpecularColor." + compositionName, compositionIndex);

                sphericalColorsKey = SphericalHarmonicsEnvironmentColorKeys.SphericalColors.ComposeIndexer("lightDiffuseColor." + compositionName, compositionIndex);
                specularCubeMapkey = RoughnessCubeMapEnvironmentColorKeys.CubeMap.ComposeIndexer("lightSpecularColor." + compositionName, compositionIndex);
                specularMipCountKey = RoughnessCubeMapEnvironmentColorKeys.MipCount.ComposeIndexer("lightSpecularColor." + compositionName, compositionIndex);
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

                // We expect the Skybox to be immutable
                if (previousSkybox == skybox)
                {
                    return;
                }

                var diffuseParameters = skybox.DiffuseLightingParameters;
                var specularParameters = skybox.SpecularLightingParameters;

                specularCubemap = specularParameters.Get(SkyboxKeys.CubeMap);
                sphericalColors = diffuseParameters.Get(SphericalHarmonicsEnvironmentColorKeys.SphericalColors);
                lightDiffuseColorShader = diffuseParameters.Get(SkyboxKeys.Shader) ?? EmptyComputeEnvironmentColorSource;
                lightSpecularColorShader = specularParameters.Get(SkyboxKeys.Shader) ?? EmptyComputeEnvironmentColorSource;

                previousSkybox = skybox;
            }

            protected override void ApplyParametersInternal(ParameterCollection parameters)
            {
                // global parameters
                parameters.Set(intensityKey, intensity);
                parameters.Set(skyMatrixKey, rotationMatrix);

                if (!ReferenceEquals(parameters.Get(lightDiffuseColorKey), lightDiffuseColorShader))
                {
                    parameters.Set(lightDiffuseColorKey, lightDiffuseColorShader);
                }
                if (!ReferenceEquals(parameters.Get(lightSpecularColorKey), lightSpecularColorShader))
                {
                    parameters.Set(lightSpecularColorKey, lightSpecularColorShader);
                }

                parameters.Set(sphericalColorsKey, sphericalColors);
                parameters.Set(specularCubeMapkey, specularCubemap);
                parameters.Set(specularMipCountKey, specularCubemap.MipLevels);
            }
        }
    }
}
