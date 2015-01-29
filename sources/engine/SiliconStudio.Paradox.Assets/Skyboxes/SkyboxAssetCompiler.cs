// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Threading.Tasks;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.ComputeEffect.GGXPrefiltering;
using SiliconStudio.Paradox.Effects.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Effects.Skyboxes;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;
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

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);
                writer.Write(1); // Change this number to recompute the hash when prefiltering algorithm are changed
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                // TODO Convert SkyboxAsset to Skybox and save to Skybox object
                // TODO Add system to prefilter
                // TODO: Add input texture as input 

                var context = new SkyboxGeneratorContext(package);
                var result = SkyboxGenerator.Compile(asset, context);

                var registry = new ServiceRegistry();
                var assetManager = new AssetManager(registry);

                // TODO: THIS PART IS TEMPORARY. TO REWRITE
                // Lambert prefiltering using Spherical Harmonics
                if (((SkyboxCubeMapModel)asset.Model).CubeMap != null)
                {
                    var graphicsDevice = GraphicsDevice.New(DeviceCreationFlags.Debug);
                    var graphicsService = new GraphicsDeviceServiceLocal(registry, graphicsDevice);
                    
                    var effectSystem = new EffectSystem(registry);
                    effectSystem.Initialize();

                    var drawFxContext = new DrawEffectContext(registry);


                    // -------------------------------------------------------------------
                    // Calculate Diffuse prefiltering
                    // -------------------------------------------------------------------
                    var lamberFiltering = new LambertianPrefilteringSH(drawFxContext);

                    var skybox = result.Skybox;

                    var location = ((SkyboxCubeMapModel)asset.Model).CubeMap.Location;
                    var skyboxTexture = assetManager.Load<Texture>(location);

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
                    bases[8] = (float)(Math.Sqrt(15.0 / PI16));
                    for (int i = 0; i < coefficients.Length; i++)
                    {
                        coefficients[i] = coefficients[i] * bases[i];
                    }

                    skybox.DiffuseLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("SphericalHarmonicsEnvironmentColor", lamberFiltering.HarmonicOrder));
                    skybox.DiffuseLightingParameters.Set(SphericalHarmonicsEnvironmentColorKeys.SphericalColors, coefficients);

                    // -------------------------------------------------------------------
                    // Calculate Specular prefiltering
                    // -------------------------------------------------------------------
                    var specularRadiancePrefilterGGX = new RadiancePrefilteringGGX(drawFxContext);

                    //var outputTexture = Texture.New2D(graphicsDevice, 256, 256, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess, 6);
                    var outputTexture = Texture.New2D(graphicsDevice, 256, 256, true, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess, 6);
                    specularRadiancePrefilterGGX.RadianceMap = skyboxTexture;
                    specularRadiancePrefilterGGX.PrefilteredRadiance = outputTexture;
                    specularRadiancePrefilterGGX.Draw();

                    var cubeTexture = Texture.NewCube(graphicsDevice, 256, true, PixelFormat.R16G16B16A16_Float);
                    graphicsDevice.Copy(outputTexture, cubeTexture);
                    using (var stream = new FileStream(Path.GetFileNameWithoutExtension(location) + "_GGX.dds", FileMode.Create, FileAccess.Write))
                    {
                        cubeTexture.Save(stream, ImageFileType.Dds);
                    }
                    cubeTexture.SetSerializationData(cubeTexture.GetDataAsImage());

                    assetManager.Save(location + "/GGX", cubeTexture);
                    skybox.SpecularLightingParameters.Set(SkyboxKeys.CubeMap, cubeTexture);
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
 
