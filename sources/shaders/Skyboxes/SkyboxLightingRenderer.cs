// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// TODO: Evaluate if it would be possible to split this class with support for different lights instead of a big fat class
    /// TODO: Refactor this class
    /// </summary>
    internal class SkyboxLightingRenderer
    {
        private readonly SkyboxProcessor skyboxProcessor;
        private readonly List<ShaderSource> shaderSources;

        private readonly List<ShaderSource> previousShaderSources;

        public SkyboxLightingRenderer(ModelRenderer modelRenderer)
        {
            if (modelRenderer == null) throw new ArgumentNullException("modelRenderer");
            Enabled = true;
            Services = modelRenderer.Services;
            EntitySystem = Services.GetServiceAs<EntitySystem>();
            skyboxProcessor = EntitySystem.GetProcessor<SkyboxProcessor>();
            shaderSources = new List<ShaderSource>();
            previousShaderSources = new List<ShaderSource>();

            modelRenderer.PreRender.Add(PreRender);
        }

        public bool Enabled { get; set; }

        public IServiceRegistry Services { get; private set; }

        private EntitySystem EntitySystem { get; set; }

        /// <summary>
        /// Filter out the inactive lights.
        /// </summary>
        /// <param name="context">The render context.</param>
        private void PreRender(RenderContext context)
        {
            shaderSources.Clear();
            var passParameters = context.CurrentPass.Parameters;

            if (Enabled)
            {
                int index = 0;
                foreach (var skyboxComponent in skyboxProcessor.ActiveSkyboxLights)
                {
                    var skybox = skyboxComponent.Skybox;

                    var diffuseParameters = skybox.DiffuseLightingParameters;
                    var specularParameters = skybox.SpecularLightingParameters;

                    var intensity = skyboxComponent.Lighting.Intensity;
                    var rotation = skyboxComponent.Lighting.Rotation;
                    var rotationMatrix = Matrix.RotationY(MathUtil.DegreesToRadians(rotation));
                    // global parameters
                    {
                        var composition = string.Format("environmentLights[{0}]", index);
                        var intensityKey = LightSkyboxKeys.Intensity.ComposeWith(composition);
                        var skyMatrixKey = LightSkyboxKeys.SkyMatrix.ComposeWith(composition);
                        passParameters.Set(intensityKey, intensity);
                        passParameters.Set(skyMatrixKey, rotationMatrix);
                    }

                    // Setup diffuse lighting
                    {
                        var cubeMapKey = LevelCubeMapEnvironmentColorKeys.CubeMap.ComposeWith(string.Format("lightDiffuseColor.environmentLights[{0}]", index));
                        var mipLevelKey = LevelCubeMapEnvironmentColorKeys.MipLevel.ComposeWith(string.Format("lightDiffuseColor.environmentLights[{0}]", index));
                        var diffuseCubemap = diffuseParameters.Get(SkyboxKeys.CubeMap);
                        passParameters.Set(cubeMapKey, diffuseCubemap);
                        passParameters.Set(mipLevelKey, diffuseCubemap.MipLevels * 0.8f);
                    }

                    // Setup specular lighting
                    {
                        var cubeMapKey = RoughnessCubeMapEnvironmentColorKeys.CubeMap.ComposeWith(string.Format("lightSpecularColor.environmentLights[{0}]", index));
                        var mipCountKey = RoughnessCubeMapEnvironmentColorKeys.MipCount.ComposeWith(string.Format("lightSpecularColor.environmentLights[{0}]", index));
                        var specularCubemap = specularParameters.Get(SkyboxKeys.CubeMap);
                        passParameters.Set(cubeMapKey, specularCubemap);
                        passParameters.Set(mipCountKey, specularCubemap.MipLevels);
                    }

                    var mixin = new ShaderMixinSource();
                    mixin.Mixins.Add(new ShaderClassSource("LightSkybox"));
                    mixin.AddComposition("lightDiffuseColor", diffuseParameters.Get(SkyboxKeys.Shader));
                    mixin.AddComposition("lightSpecularColor", specularParameters.Get(SkyboxKeys.Shader));
                    shaderSources.Add(mixin);

                    passParameters.Set(SkyboxKeys.CubeMap, diffuseParameters.Get(SkyboxKeys.CubeMap));
                    break;
                }

                bool hasShaderSourcesChanges = false;
                if (shaderSources.Count == previousShaderSources.Count)
                {
                    for (int i = 0; i < shaderSources.Count; i++)
                    {
                        if (!shaderSources[i].Equals(previousShaderSources[i]))
                        {
                            hasShaderSourcesChanges = true;
                        }
                    }
                }
                else
                {
                    hasShaderSourcesChanges = true;
                }

                if (hasShaderSourcesChanges)
                {
                    passParameters.Set(LightingKeys.EnvironmentLights, shaderSources.ToArray());
                }
            }
            else
            {
                if (passParameters.Get(LightingKeys.EnvironmentLights) != null)
                {
                    passParameters.Set(LightingKeys.EnvironmentLights, null);
                }
            }

            previousShaderSources.Clear();
            previousShaderSources.AddRange(shaderSources);
            shaderSources.Clear();
        }
    }
}
