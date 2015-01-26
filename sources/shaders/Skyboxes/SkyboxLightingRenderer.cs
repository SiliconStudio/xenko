// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
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

        public SkyboxLightingRenderer(ModelRenderer modelRenderer)
        {
            if (modelRenderer == null) throw new ArgumentNullException("modelRenderer");
            Services = modelRenderer.Services;
            EntitySystem = Services.GetServiceAs<EntitySystem>();
            skyboxProcessor = EntitySystem.GetProcessor<SkyboxProcessor>();
            shaderSources = new List<ShaderSource>();

            modelRenderer.PreRender.Add(PreRender);
        }

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

            if (skyboxProcessor.Enabled)
            {
                foreach (var skybox in skyboxProcessor.ActiveSkyboxLights)
                {
                    var skylightParameters = skybox.Skybox.Parameters;

                    var environmentShader = skylightParameters.Get(SkyboxKeys.Shader);

                    var mixin = new ShaderMixinSource();
                    mixin.Mixins.Add(new ShaderClassSource("LightSkybox"));
                    mixin.AddComposition("lightDiffuseColor", environmentShader);
                    shaderSources.Add(mixin);

                    passParameters.Set(SkyboxKeys.CubeMap, skylightParameters.Get(SkyboxKeys.CubeMap));
                    break;
                }

                passParameters.Set(LightingKeys.EnvironmentLights, shaderSources.ToArray());
            }
            else
            {
                passParameters.Set(LightingKeys.EnvironmentLights, null);
            }
        }
    }
}
