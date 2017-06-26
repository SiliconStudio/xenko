// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SiliconStudio.Xenko.Engine.Xaml;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Xaml;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering;
#if XENKO_YEBIS
using SiliconStudio.Xenko.Rendering.Yebis;
#endif
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Configuration;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.ObjectModel;
using ScriptShader.Effects;

namespace ScriptTest
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RenderingSetup
    {
        private bool lightingRegistered;

        public static readonly RenderingSetup Singleton = new RenderingSetup();
        private IRenderSystem renderSystem;
        private IGraphicsDeviceService graphicsDeviceService;
        private IEffectSystemOld effectSystemOld;
        private IEntitySystem entitySystem;

        private RenderConfigContext RenderConfigContext { get; set; }

        public RenderTargetsPlugin MainTargetPlugin { get; set; }
        public MainPlugin MainPlugin { get; set; }

        public void Initialize(IServiceRegistry registry, string effectFilename = null, string[] optionalFeatures = null)
        {
            // Missing features compared to before: ZInverse support, Picking/Wireframe, Heat Shimmering and light shafts bounding boxes.
            // Other stuff to implement: Enable features by RenderPipeline, reloading, access of plugins through a flexible interface, yebis config.
            renderSystem = registry.GetSafeServiceAs<IRenderSystem>();
            graphicsDeviceService = registry.GetSafeServiceAs<IGraphicsDeviceService>();
            this.effectSystemOld = registry.GetSafeServiceAs<IEffectSystemOld>();
            entitySystem = registry.GetSafeServiceAs<IEntitySystem>();
            
            var rootRenderPass = renderSystem.RootRenderPass;
            var dataContext = RenderConfigContext = renderSystem.ConfigContext;
            var graphicsDevice = graphicsDeviceService.GraphicsDevice;

            if (effectFilename == null)
                effectFilename = Path.Combine("/shaders/effects.xml");

            var context = new XenkoXamlSchemaContext(dataContext);
            var xamlObjectWriter = new XamlObjectWriter(context);

            using (var fileStream = VirtualFileSystem.OpenStream(effectFilename, VirtualFileMode.Open, VirtualFileAccess.Read))
                XamlServices.Transform(new XamlXmlReader(fileStream, context), xamlObjectWriter);

            var effectConfig = (RenderConfig)xamlObjectWriter.Result;

            foreach (var renderPass in effectConfig.Content.OfType<RenderPass>())
            {
                dataContext.RenderPasses.Add(renderPass.Name, renderPass);
                rootRenderPass.AddPass(renderPass);
            }

            foreach (var item in effectConfig.Content)
            {
                var plugin = item as RenderPassPlugin;
                if (plugin != null)
                {
                    dataContext.RenderPassPlugins.Add(plugin.Name, plugin);
                }

                var setter = item as Setter;
                if (setter != null)
                {
                    PropertyPath.SetNextValue(setter.Target, setter.Property, setter.Value);
                }
            }

            MainPlugin = dataContext.RenderPassPlugins.Select(x => x.Value).OfType<MainPlugin>().First();
            MainTargetPlugin = dataContext.RenderPassPlugins.Select(x => x.Value).OfType<RenderTargetsPlugin>().FirstOrDefault(x => x.Name == "MainTargetPlugin");

            var mainBackBuffer = graphicsDevice.BackBuffer;
            MainPlugin.RenderTarget = graphicsDevice.BackBuffer;
            
            // Depth Stencil target needs to be shader resource only if Yebis or GBuffer is active (need more robust way to decide)
            var depthStencilTexture = Texture.New2D(graphicsDevice, mainBackBuffer.Width, mainBackBuffer.Height, PixelFormat.D32_Float,
                (RenderConfigContext.RenderPassPlugins.Any(x => x.Value is YebisPlugin || x.Value is GBufferPlugin) ? TextureFlags.ShaderResource : 0) | TextureFlags.DepthStencil);
            MainPlugin.DepthStencil = depthStencilTexture.ToDepthStencilBuffer(false);

            if (DepthStencilBuffer.IsReadOnlySupported(graphicsDevice))
                MainPlugin.DepthStencilReadOnly = depthStencilTexture.ToDepthStencilBuffer(true);

            // TODO: Temporary setup (should be done through an Entity and its Manager)
            HeatShimmerPlugin heatShimmerPlugin;
            if (RenderConfigContext.RenderPassPlugins.TryGetValueCast("HeatShimmerPlugin", out heatShimmerPlugin))
            {
                throw new NotImplementedException();
                //heatShimmerPlugin.BoundingBoxes.Add(new MeshData { MeshData = MeshDataHelper.CreateBox(1, 1, 1, Color.White, true), Parameters = new ParameterCollectionData { { TransformationKeys.World, Matrix.Scaling(8200, 3000, 1500) * Matrix.Translation(2700, 0, 300) } } });
                //heatShimmerPlugin.BoundingBoxes.Add(new MeshData { MeshData = MeshDataHelper.CreateBox(1, 1, 1, Color.White, true), Parameters = new ParameterCollectionData { { TransformationKeys.World, Matrix.Scaling(2000, 2000, 3500) * Matrix.RotationZ(0.5f) * Matrix.Translation(-7000, -4000, 1500) } } });
                //heatShimmerPlugin.BoundingBoxes.Add(new MeshData { MeshData = MeshDataHelper.CreateBox(1, 1, 1, Color.White, true), Parameters = new ParameterCollectionData { { TransformationKeys.World, Matrix.Scaling(2000, 3000, 3500) * Matrix.Translation(-7800, 900, 1500) } } });
            }
            
            // Generates intermediate render targets
            var plugins = dataContext.RenderPassPlugins
                .OrderBy(x => rootRenderPass.Passes.IndexOf(x.Value.RenderPass)).ToArray();

            // Weave render targets from last to first plugin.
            // TODO: Instead of guessing through interface and non-null/null values, it would be better if plugin had flags to inform of its intentions.
            var currentTarget = mainBackBuffer;
            for (int i = plugins.Length - 1; i >= 0; --i)
            {
                var plugin = plugins[i];

                var targetPlugin = plugin.Value as IRenderPassPluginTarget;
                if (targetPlugin != null)
                {
                    if (targetPlugin.RenderTarget == null)
                        targetPlugin.RenderTarget = currentTarget;

                    currentTarget = targetPlugin.RenderTarget;
                }

                var sourcePlugin = plugin.Value as IRenderPassPluginSource;
                if (sourcePlugin != null)
                {
                    if (sourcePlugin.RenderSource == null)
                    {
                        sourcePlugin.RenderSource = Texture.New2D(graphicsDevice, mainBackBuffer.Width, mainBackBuffer.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                    }

                    currentTarget = sourcePlugin.RenderSource.ToRenderTarget();
                }
            }

            foreach (var plugin in dataContext.RenderPassPlugins)
            {
                renderSystem.RenderPassPlugins.Add(plugin.Value);
            }

            foreach (var effectBuilder in effectConfig.Content.OfType<EffectBuilder>())
            {
                foreach (var plugin in effectBuilder.Plugins)
                {
                    plugin.Services = registry;
                }
                this.effectSystemOld.Effects.Add(effectBuilder);
            }

#if XENKO_YEBIS
            YebisPlugin yebisPlugin;
            if (RenderConfigContext.RenderPassPlugins.TryGetValueCast("YebisPlugin", out yebisPlugin))
            {
                yebisPlugin.AntiAlias = true;

                yebisPlugin.ToneMap.Gamma = 2.2f;
                yebisPlugin.ToneMap.Type = ToneMapType.SensiToMetric;
                yebisPlugin.ToneMap.AutoExposure.Enable = true;
                yebisPlugin.ToneMap.AutoExposure.MiddleGray = 0.25f;
                yebisPlugin.ToneMap.AutoExposure.AdaptationSensitivity = 0.5f;
                yebisPlugin.ToneMap.AutoExposure.AdaptationScale = 0.8f;
                yebisPlugin.ToneMap.AutoExposure.AdaptationSpeedLimit = 4.0f;
                yebisPlugin.ToneMap.AutoExposure.DarkAdaptationSensitivity = 0.9f;
                yebisPlugin.ToneMap.AutoExposure.DarkAdaptationScale = 0.6f;
                yebisPlugin.ToneMap.AutoExposure.DarkAdaptationSpeedLimit = 4.0f;
                yebisPlugin.ToneMap.AutoExposure.LightDarkExposureBorder = 1.0f;

                yebisPlugin.Glare.Enable = true;
                //yebisPlugin.Glare.RemapFactor = 1.0f;
                //yebisPlugin.Glare.Threshold = 0.0f;

                yebisPlugin.Lens.Vignette.Enable = true;

                yebisPlugin.Lens.Distortion.Enable = false;
                yebisPlugin.Lens.Distortion.Power = 0.2f;
                yebisPlugin.Lens.Distortion.EdgeSmoothness = 0.2f;

                yebisPlugin.DepthOfField.Enable = true;
                yebisPlugin.DepthOfField.AutoFocus = true;
                yebisPlugin.DepthOfField.Aperture = 2.0f;
                yebisPlugin.DepthOfField.ImageSensorHeight = 40.0f;

                //yebisPlugin.ColorCorrection.ColorTemperature = 4500;

                yebisPlugin.HeatShimmer.Enable = false;
                //YebisPlugin.LightShaft.Enable = true;
                //YebisPlugin.LightShaft.ScreenPosition = new Vector2(0.5f, 0.1f);
            }
#endif

            // Adds the particle system if the ParticlePlugin is used in the config
            ParticlePlugin particlePlugin;
            if (RenderConfigContext.RenderPassPlugins.TryGetValueCast("ParticlePlugin", out particlePlugin))
            {
                var particleSystem = new ParticleProcessor(particlePlugin);
                entitySystem.Processors.Add(particleSystem);
            }
        }

        public bool ToggleWireframe()
        {
            RenderTargetsPlugin wireframePlugin;
            if (RenderConfigContext.RenderPassPlugins.TryGetValueCast("WireframePlugin", out wireframePlugin))
                return wireframePlugin.RenderPass.Enabled = !wireframePlugin.RenderPass.Enabled;

            return false;
        }
        
        public void RegisterLighting()
        {
            if (lightingRegistered)
                return;

            lightingRegistered = true;

            // Add LightProcessor if used in effect config file
            LightingPrepassPlugin lightingPrepassPlugin;
            LightingPlugin lightingPlugin;
            RenderConfigContext.RenderPassPlugins.TryGetValueCast("LightingPrepassPlugin", out lightingPrepassPlugin);
            RenderConfigContext.RenderPassPlugins.TryGetValueCast("LightingPlugin", out lightingPlugin);
            if (lightingPrepassPlugin != null || lightingPlugin != null)
            {
                RenderTargetsPlugin editorTargetPlugin;
                RenderConfigContext.RenderPassPlugins.TryGetValueCast("EditorTargetPlugin", out editorTargetPlugin);
                var lightProcessor = new LightProcessor(lightingPlugin, editorTargetPlugin, lightingPrepassPlugin != null ? lightingPrepassPlugin.Lights : null, false);
                entitySystem.Processors.Add(lightProcessor);

                // LightShafts enabled?
                RenderPass lightShaftsPass;
                if (RenderConfigContext.RenderPasses.TryGetValue("LightShaftsPass", out lightShaftsPass))
                    entitySystem.Processors.Add(new LightShaftsProcessor(MainPlugin, MainTargetPlugin, lightShaftsPass));

                entitySystem.Processors.Add(new LightReceiverProcessor(lightProcessor));
            }
        }
    }
}
