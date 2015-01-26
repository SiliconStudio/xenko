// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Effects.Skyboxes;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Skyboxes
{
    public class SkyboxGeneratorContext : ShaderGeneratorContextBase
    {
        public SkyboxGeneratorContext()
        {
        }

        public SkyboxGeneratorContext(Package package)
            : base(package)
        {
        }
    }

    public class SkyboxResult : LoggerResult
    {
        public Skybox Skybox { get; set; }
    }

    public class SkyboxGenerator
    {
        public static SkyboxResult Compile(SkyboxAsset asset, SkyboxGeneratorContext context)
        {
            if (asset == null) throw new ArgumentNullException("asset");
            if (context == null) throw new ArgumentNullException("context");
            var result = new SkyboxResult { Skybox = new Skybox() };

            var parameters = context.Parameters;
            var skybox = result.Skybox;
            skybox.Parameters = parameters;

            if (asset.Model != null)
            {
                var shaderSource = asset.Model.Generate(context);
                parameters.Set(SkyboxKeys.Shader, shaderSource);

                skybox.DiffuseLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("LevelCubeMapEnvironmentColor"));
                skybox.DiffuseLightingParameters.Set(SkyboxKeys.CubeMap, parameters.Get(SkyboxKeys.CubeMap));

                skybox.SpecularLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("RoughnessCubeMapEnvironmentColor"));
                skybox.SpecularLightingParameters.Set(SkyboxKeys.CubeMap, parameters.Get(SkyboxKeys.CubeMap));
            }

            return result;
        }
         
    }
}