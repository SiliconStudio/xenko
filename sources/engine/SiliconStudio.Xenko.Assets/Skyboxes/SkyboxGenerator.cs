// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.ComputeEffect.GGXPrefiltering;
using SiliconStudio.Xenko.Rendering.ComputeEffect.LambertianPrefiltering;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Assets.Skyboxes
{
    public class SkyboxGeneratorContext : ShaderGeneratorContext, IDisposable
    {
        public SkyboxGeneratorContext(SkyboxAsset skybox)
        {
            if (skybox == null) throw new ArgumentNullException(nameof(skybox));

            Skybox = skybox;
            Services = new ServiceRegistry();
            Content = new ContentManager(Services);

            GraphicsDevice = GraphicsDevice.New();
            GraphicsDeviceService = new GraphicsDeviceServiceLocal(Services, GraphicsDevice);

            var graphicsContext = new GraphicsContext(GraphicsDevice);
            Services.AddService(typeof(GraphicsContext), graphicsContext);

            EffectSystem = new EffectSystem(Services);
            EffectSystem.Initialize();
            ((IContentable)EffectSystem).LoadContent();
            ((EffectCompilerCache)EffectSystem.Compiler).CompileEffectAsynchronously = false;

            RenderContext = RenderContext.GetShared(Services);
            RenderDrawContext = new RenderDrawContext(Services, RenderContext, graphicsContext);
        }

        public IServiceRegistry Services { get; private set; }

        public EffectSystem EffectSystem { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public IGraphicsDeviceService GraphicsDeviceService { get; private set; }

        public RenderContext RenderContext { get; private set; }

        public RenderDrawContext RenderDrawContext { get; private set; }

        public SkyboxAsset Skybox { get; }

        public void Dispose()
        {
            EffectSystem.Dispose();
            GraphicsDevice.Dispose();
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
            
            var cubemap = asset.CubeMap;
            if (cubemap == null)
            {
                return result;
            }

            // load the skybox texture from the asset.
            var reference = AttachedReferenceManager.GetAttachedReference(cubemap);
            var skyboxTexture = context.Content.Load<Texture>(BuildTextureForSkyboxGenerationLocation(reference.Url));
            if (skyboxTexture.Dimension != TextureDimension.TextureCube)
            {
                result.Error("SkyboxGenerator: The texture used as skybox should be a Cubemap.");
                return result;
            }

            // If we are using the skybox asset for lighting, we can compute it
            // Specular lighting only?
            if (!asset.IsSpecularOnly)
            {
                // -------------------------------------------------------------------
                // Calculate Diffuse prefiltering
                // -------------------------------------------------------------------
                var lamberFiltering = new LambertianPrefilteringSHNoCompute(context.RenderContext)
                {
                    HarmonicOrder = (int)asset.DiffuseSHOrder,
                    RadianceMap = skyboxTexture
                };
                lamberFiltering.Draw(context.RenderDrawContext);

                var coefficients = lamberFiltering.PrefilteredLambertianSH.Coefficients;
                for (int i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i] = coefficients[i]*SphericalHarmonics.BaseCoefficients[i];
                }

                skybox.DiffuseLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("SphericalHarmonicsEnvironmentColor", lamberFiltering.HarmonicOrder));
                skybox.DiffuseLightingParameters.Set(SphericalHarmonicsEnvironmentColorKeys.SphericalColors, coefficients);
            }

            // -------------------------------------------------------------------
            // Calculate Specular prefiltering
            // -------------------------------------------------------------------
            var specularRadiancePrefilterGGX = new RadiancePrefilteringGGXNoCompute(context.RenderContext);

            var textureSize = asset.SpecularCubeMapSize <= 0 ? 64 : asset.SpecularCubeMapSize;
            textureSize = (int)Math.Pow(2, Math.Round(Math.Log(textureSize, 2)));
            if (textureSize < 64) textureSize = 64;

            // TODO: Add support for HDR 32bits 
            var filteringTextureFormat = skyboxTexture.Format.IsHDR() ? skyboxTexture.Format : PixelFormat.R8G8B8A8_UNorm;

            //var outputTexture = Texture.New2D(graphicsDevice, 256, 256, skyboxTexture.Format, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess, 6);
            using (var outputTexture = Texture.New2D(context.GraphicsDevice, textureSize, textureSize, true, filteringTextureFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget, 6))
            {
                specularRadiancePrefilterGGX.RadianceMap = skyboxTexture;
                specularRadiancePrefilterGGX.PrefilteredRadiance = outputTexture;
                specularRadiancePrefilterGGX.Draw(context.RenderDrawContext);

                var cubeTexture = Texture.NewCube(context.GraphicsDevice, textureSize, true, skyboxTexture.Format);
                context.RenderDrawContext.CommandList.Copy(outputTexture, cubeTexture);

                cubeTexture.SetSerializationData(cubeTexture.GetDataAsImage(context.RenderDrawContext.CommandList));

                skybox.SpecularLightingParameters.Set(SkyboxKeys.Shader, new ShaderClassSource("RoughnessCubeMapEnvironmentColor"));
                skybox.SpecularLightingParameters.Set(SkyboxKeys.CubeMap, cubeTexture);
            }
            // TODO: cubeTexture is not deallocated

            return result;
        }

        public static string BuildTextureForSkyboxGenerationLocation(string textureLocation)
        {
            return textureLocation + "__ForSkyboxCompilation__";
        }
    }
}
