// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Effects.Skyboxes;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Skyboxes
{
    internal class SkyboxAssetCompiler : AssetCompilerBase<SkyboxAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, SkyboxAsset asset, AssetCompilerResult result)
        {
            result.ShouldWaitForPreviousBuilds = true;
            result.BuildSteps = new ListBuildStep { new SkyboxCompileCommand(urlInStorage, asset, context.Package) };
        }

        private class SkyboxCompileCommand : AssetCommand<SkyboxAsset>
        {
            private readonly Package package;

            public SkyboxCompileCommand(string url, SkyboxAsset asset, Package package)
                : base(url, asset)
            {
                this.package = package;
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // TODO Convert SkyboxAsset to Skybox and save to Skybox object
                // TODO Add system to prefilter

                var context = new SkyboxGeneratorContext(package);
                var result = SkyboxGenerator.Compile(asset, context);

                var registry = new ServiceRegistry();
                var assetManager = new AssetManager(registry);

                // TODO: THIS PART IS TEMPORARY. TO REWRITE
                // Lambert prefiltering using Spherical Harmonics
                if (((SkyboxCubeMapModel)asset.Model).CubeMap != null)
                {
                    var graphicsDevice = GraphicsDevice.New();
                    var graphicsService = new GraphicsDeviceServiceLocal(registry, graphicsDevice);
                    
                    var effectSystem = new EffectSystem(registry);
                    effectSystem.Initialize();

                    var drawFxContext = new DrawEffectContext(registry);

                    var lamberFiltering = new LambertianPrefilteringSH(drawFxContext);

                    var skybox = result.Skybox;

                    var skyboxTexture = assetManager.Load<Texture>(((SkyboxCubeMapModel)asset.Model).CubeMap.Location);

                    lamberFiltering.HarmonicOrder = 3;
                    lamberFiltering.RadianceMap = skyboxTexture;
                    lamberFiltering.Draw();

                    var PI4 = 4 * Math.PI;
                    var PI16 =16 * Math.PI;
                    var PI64 =64 * Math.PI;        
                    var SQRT_PI  = 1.77245385090551602729;

                    var coefficients = lamberFiltering.PrefilteredLambertianSH.Coefficients;

                    var bases = new float[coefficients.Length];
                    bases[0] = (float)(1.0 / (2.0 * SQRT_PI));

                    bases[1] = (float)(-Math.Sqrt(3.0 / PI4));
                    bases[2] = (float)(Math.Sqrt(3.0 / PI4));
                    bases[3] = (float)(-Math.Sqrt(3.0 / PI4));

                    bases[4] = (float)(Math.Sqrt(15.0 / PI4));
                    bases[5] = (float)(-Math.Sqrt(15.0 / PI4));
                    bases[6] = (float)(Math.Sqrt(5.0 / PI16));
                    bases[7] = (float)(-Math.Sqrt(15.0 / PI4));
                    bases[8] = (float)(Math.Sqrt(15.0 / PI4));
                    for (int i = 0; i < coefficients.Length; i++)
                    {
                        coefficients[i] = coefficients[i] * bases[i];
                    }

                    skybox.DiffuseLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("SphericalHarmonicsEnvironmentColor", lamberFiltering.HarmonicOrder));
                    skybox.DiffuseLightingParameters.Set(SphericalHarmonicsEnvironmentColorKeys.SphericalColors, coefficients);
                }

                if (result.HasErrors)
                {
                    result.CopyTo(commandContext.Logger);
                    return Task.FromResult(ResultStatus.Failed);
                }

                assetManager.Save(Url, result.Skybox);

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
 
