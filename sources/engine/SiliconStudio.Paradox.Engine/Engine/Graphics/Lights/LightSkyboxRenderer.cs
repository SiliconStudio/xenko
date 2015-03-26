// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
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
        private const int LightMax = 8;

        private readonly List<LightShaderGroup> defaultLightShaders;
        private readonly List<LightShaderGroup> currentLightShaders;

        private const string LightDiffuseColorComposition = "lightDiffuseColor";
        private const string LightSpecularColorComposition = "lightSpecularColor";

        private static readonly ParameterKey<Color3[]> SphericalColorsKey = SphericalHarmonicsEnvironmentColorKeys.SphericalColors.ComposeWith(LightDiffuseColorComposition);
        private static readonly ParameterKey<Texture> SpecularCubeMapkey = RoughnessCubeMapEnvironmentColorKeys.CubeMap.ComposeWith(LightSpecularColorComposition);
        private static readonly ParameterKey<float> SpecularMipCount = RoughnessCubeMapEnvironmentColorKeys.MipCount.ComposeWith(LightSpecularColorComposition);
        private static readonly ShaderClassSource EmptyComputeEnvironmentColorSource = new ShaderClassSource("IComputeEnvironmentColor");


        public LightSkyboxRenderer()
        {
            defaultLightShaders = new List<LightShaderGroup>();
            currentLightShaders = new List<LightShaderGroup>();

            // Precreate a fixed list of skybox light shader group. Usually there is only one.
            for (int i = 0; i < LightMax; i++)
            {
                var mixin = new ShaderMixinSource();
                mixin.Mixins.Add(new ShaderClassSource("LightSkyboxShader"));
                mixin.AddComposition(LightDiffuseColorComposition, null);
                mixin.AddComposition(LightSpecularColorComposition, null);
                defaultLightShaders.Add(new LightShaderGroup(mixin));
            }
        }

        public override bool IsDirectLight
        {
            get
            {
                return false;
            }
        }

        public override List<LightShaderGroup> PrepareLights(RenderContext context, LightComponentCollection lights, bool isLightWithShadow)
        {
            var count = Math.Min(lights.Count, LightMax);

            currentLightShaders.Clear();
            for (int i = 0; i < count; i++)
            {
                var lightComponent = lights[i];

                // TODO: If there is a performance penalty for accessing the SkyboxComponent, this could be prepared by the LightProcessor
                var skyboxComponent = lightComponent.Entity.Get<SkyboxComponent>();
                if (skyboxComponent == null || skyboxComponent.Skybox == null)
                {
                    continue;
                }

                var skybox = skyboxComponent.Skybox;

                var lightShaderGroup = defaultLightShaders[currentLightShaders.Count];

                var diffuseParameters = skybox.DiffuseLightingParameters;
                var specularParameters = skybox.SpecularLightingParameters;

                var diffuseShader = diffuseParameters.Get(SkyboxKeys.Shader);
                var specularCubemap = specularParameters.Get(SkyboxKeys.CubeMap);

                // No diffuse nor specular shader, let's ignore this cubemap
                if (diffuseShader == null && specularCubemap == null)
                    continue;

                var intensity = lightComponent.Intensity;
                if (skyboxComponent.Enabled)
                {
                    intensity *= skyboxComponent.Intensity;
                }

                var rotationMatrix = Matrix.RotationQuaternion(lightComponent.Entity.Transform.Rotation);

                // global parameters
                lightShaderGroup.Parameters.Set(LightSkyboxShaderKeys.Intensity, intensity);
                lightShaderGroup.Parameters.Set(LightSkyboxShaderKeys.SkyMatrix, rotationMatrix);

                // Setup diffuse lighting
                var mixinSource = (ShaderMixinSource)lightShaderGroup.ShaderSource;

                mixinSource.Compositions[LightDiffuseColorComposition] = diffuseShader ?? EmptyComputeEnvironmentColorSource;
                // TODO: Use pluggable keys instead of copying parameters
                lightShaderGroup.Parameters.Set(SphericalColorsKey, diffuseParameters.Get(SphericalHarmonicsEnvironmentColorKeys.SphericalColors));

                // Setup specular lighting
                if (specularCubemap != null)
                {
                    mixinSource.Compositions[LightSpecularColorComposition] = specularParameters.Get(SkyboxKeys.Shader);

                    // TODO: Use pluggable keys instead of copying manually parameters
                    lightShaderGroup.Parameters.Set(SpecularCubeMapkey, specularCubemap);
                    lightShaderGroup.Parameters.Set(SpecularMipCount, specularCubemap.MipLevels);
                }
                else
                {
                    mixinSource.Compositions[LightDiffuseColorComposition] = EmptyComputeEnvironmentColorSource;
                    lightShaderGroup.Parameters.Set(SpecularCubeMapkey, null);
                    lightShaderGroup.Parameters.Set(SpecularMipCount, 0);
                }

                currentLightShaders.Add(lightShaderGroup);
            }

            return currentLightShaders;
        }
    }
}
