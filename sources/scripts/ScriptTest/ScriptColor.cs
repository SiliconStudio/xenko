// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Effects;
#if XENKO_YEBIS
using Xenko.Effects.Yebis;
#endif
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.Mathematics;
using Xenko.Framework.Shaders;

namespace ScriptTest
{
    [XenkoScript]
    public class ScriptColor
    {
        public void Dispose()
        {
            //depthTextureHolder.Release();
        }

        class Sphere
        {
            public EffectMesh Mesh;
            public float Phase;
            public float Speed;
        }

        struct LightInfo
        {
            public float Radius;
            public float Phase;
            public float Z;
        }


        public class RenderingSetup
        {
            public static readonly RenderingSetup Singleton = new RenderingSetup();

            public PostEffectPlugin LinearColorPlugin { get; set; }

            public PostEffectPlugin ReinhardColorPlugin { get; set; }

#if XENKO_YEBIS
            public YebisPlugin YebisPlugin { get; set; }
#endif

            public PostEffectPlugin FilmicColorPlugin { get; set; }

            public PostEffectPlugin MainPlugin { get; set; }

            public void Initialize(EngineContext engineContext)
            {
                var renderContext = engineContext.RenderContext;
                var rootRenderPass = renderContext.RootRenderPass;

                var linearColorPass = new RenderPass("LinearColor");
                var reinhardColorPass = new RenderPass("ReinhardColor");
                var yebisPass = new RenderPass("YebisColor");
                var filmicColorPass = new RenderPass("FilmicColor");
                var composeColorPass = new RenderPass("ComposeColor");

                rootRenderPass.AddPass(linearColorPass);
                rootRenderPass.AddPass(reinhardColorPass);
                rootRenderPass.AddPass(yebisPass);
                rootRenderPass.AddPass(filmicColorPass);
                rootRenderPass.AddPass(composeColorPass);

                LinearColorPlugin = new PostEffectPlugin("LinearColor") { RenderPass = linearColorPass };

                ReinhardColorPlugin = new PostEffectPlugin("ReinhardColor") { RenderPass = reinhardColorPass };

#if XENKO_YEBIS
                YebisPlugin = new YebisPlugin("ReinhardColor") { RenderPass = yebisPass };
#endif

                FilmicColorPlugin = new PostEffectPlugin("FilmicColor") { RenderPass = filmicColorPass };

                MainPlugin = new PostEffectPlugin("MainColor") { RenderPass = composeColorPass };

                //MainDepthReadOnlyPlugin

                // YebisPlugin = new YebisPlugin() { RenderPass = yebisColorPass, MainDepthReadOnlyPlugin = null };

                //renderContext.Register(MainDepthReadOnlyPlugin);
                //renderContext.Register(SkyBoxPlugin);
                //renderContext.Register(PostEffectPlugin);

                //// Create and bind depth stencil buffer
                //renderContext.GraphicsResizeContext.SetupResize(
                //    (resizeContext) =>
                //    {
                //        MainDepthReadOnlyPlugin.DepthStencil = renderContext.GraphicsDevice.DepthStencilBuffer.New(DepthFormat.Depth32, renderContext.Width, renderContext.Height, true, "MainDepthBuffer");
                //        // Bind render target - comment when Yebis is off
                //        MainDepthReadOnlyPlugin.RenderTarget = renderContext.GraphicsDevice.RenderTarget2D.New(renderContext.Width, renderContext.Height, PixelFormat.HalfVector4, name: "MainRenderTarget");

                //        // Comment - when Yebis is on
                //        //MainDepthReadOnlyPlugin.RenderTarget = renderContext.GraphicsDevice.RenderTarget2D.New(renderContext.Width, renderContext.Height, PixelFormat.R8G8B8A8, name: "MainRenderTarget");
                //        //renderContext.GlobalPass.EndPass.AddFirst = threadContext => threadContext.GraphicsDevice.Copy(MainDepthReadOnlyPlugin.RenderTarget, engineContext.RenderContext.RenderTarget);
                //    });

                //// Yebis plugin must be initialized after creating MainDepthReadOnlyPlugin.RenderTarget
                //renderContext.Register(YebisPlugin);

                //YebisPlugin.ToneMap.Gamma = 1.0f;
                //YebisPlugin.ToneMap.Type = ToneMapType.Auto;
                //YebisPlugin.ToneMap.AutoExposure.MiddleGray = 0.25f;
                //YebisPlugin.ToneMap.AutoExposure.AdaptationSensitivity = 0.5f;
                //YebisPlugin.ToneMap.AutoExposure.AdaptationScale = 0.8f;
                //YebisPlugin.ToneMap.AutoExposure.AdaptationSpeedLimit = 4.0f;
                //YebisPlugin.ToneMap.AutoExposure.DarkAdaptationSensitivity = 0.9f;
                //YebisPlugin.ToneMap.AutoExposure.DarkAdaptationScale = 0.6f;
                //YebisPlugin.ToneMap.AutoExposure.DarkAdaptationSpeedLimit = 4.0f;
                //YebisPlugin.ToneMap.AutoExposure.LightDarkExposureBorder = 1.0f;

                //YebisPlugin.Glare.Enable = true;
                //YebisPlugin.Glare.RemapFactor = 1f;
                //YebisPlugin.Glare.Threshold = 0f;

                //YebisPlugin.Lens.Vignette.Enable = true;

                //YebisPlugin.ColorCorrection.ColorTemperature = 3500;
            }
        }


        [XenkoScript]
        public static async Task Run(EngineContext engineContext)
        {
            var renderingSetup = new RenderingSetup();
            renderingSetup.Initialize(engineContext);
            var device = engineContext.RenderContext.GraphicsDevice;

            var effectMeshGroup = new RenderPassListEnumerator();
            engineContext.RenderContext.RenderPassEnumerators.Add(effectMeshGroup);


            EffectOld linearEffect = engineContext.RenderContext.BuildEffect("LinearColor")
                        .Using(new PostEffectShaderPlugin())
                        .Using(new BasicShaderPlugin(new ShaderClassSource("ComputeToneMap", "0")));
            var linearMesh = new EffectMesh(linearEffect);
            renderingSetup.LinearColorPlugin.RenderPass.AddPass(linearMesh.EffectMeshPasses[0].EffectPass);
            var linearTexture = Texture2D.New(engineContext.RenderContext.GraphicsDevice, 512, 512, PixelFormat.R16G16B16A16_Float, TextureFlags.RenderTarget);
            linearMesh.Parameters.Set(RenderTargetKeys.RenderTarget, linearTexture.ToRenderTarget());
            effectMeshGroup.AddMesh(linearMesh);

            EffectOld reinhardColor = engineContext.RenderContext.BuildEffect("ReinhardColor")
                        .Using(new PostEffectShaderPlugin())
                        .Using(new BasicShaderPlugin(new ShaderClassSource("ComputeToneMap", "1")));
            var reinhardMesh = new EffectMesh(reinhardColor);
            renderingSetup.ReinhardColorPlugin.RenderPass.AddPass(reinhardMesh.EffectMeshPasses[0].EffectPass);
            var reinhardTexture = Texture2D.New(engineContext.RenderContext.GraphicsDevice, 512, 512, PixelFormat.R16G16B16A16_Float, TextureFlags.RenderTarget);
            reinhardMesh.Parameters.Set(RenderTargetKeys.RenderTarget, reinhardTexture.ToRenderTarget());
            effectMeshGroup.AddMesh(reinhardMesh);

            var yebisTexture = Texture2D.New(engineContext.RenderContext.GraphicsDevice, 512, 512, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget);
#if XENKO_YEBIS
            var yebisPlugin = renderingSetup.YebisPlugin;

            yebisPlugin.RenderSource = reinhardTexture;
            yebisPlugin.RenderTarget = yebisTexture.ToRenderTarget();

            yebisPlugin.Glare.Enable = false;
            yebisPlugin.Lens.Vignette.Enable = false;
            yebisPlugin.Lens.Distortion.Enable = false;

            yebisPlugin.ToneMap.Exposure = 1.0f;
            yebisPlugin.ToneMap.Gamma = 2.2f;
            yebisPlugin.ToneMap.Type = ToneMapType.Reinhard;
            engineContext.RenderContext.Register(yebisPlugin);
#endif

            EffectOld filmicEffect = engineContext.RenderContext.BuildEffect("FilmicColor")
                        .Using(new PostEffectShaderPlugin())
                        .Using(new BasicShaderPlugin(new ShaderClassSource("ComputeToneMap", "2")));
            var filmicMesh = new EffectMesh(filmicEffect);
            renderingSetup.FilmicColorPlugin.RenderPass.AddPass(filmicMesh.EffectMeshPasses[0].EffectPass);
            var filmicTexture = Texture2D.New(engineContext.RenderContext.GraphicsDevice, 512, 512, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            filmicTexture.Name = "FilmicTexture";
            filmicMesh.Parameters.Set(RenderTargetKeys.RenderTarget, filmicTexture.ToRenderTarget());
            effectMeshGroup.AddMesh(filmicMesh);

            EffectOld mainEffect = engineContext.RenderContext.BuildEffect("ComposeToneMap")
            .Using(new PostEffectShaderPlugin())
            .Using(new BasicShaderPlugin(new ShaderClassSource("ComposeToneMap")));
            var mainMesh = new EffectMesh(mainEffect);
            renderingSetup.MainPlugin.RenderPass.AddPass(mainMesh.EffectMeshPasses[0].EffectPass);

            mainMesh.Parameters.Set(TexturingKeys.Texture0, linearTexture);
            mainMesh.Parameters.Set(TexturingKeys.Texture2, yebisTexture);
            mainMesh.Parameters.Set(TexturingKeys.Texture3, filmicTexture);
            mainMesh.Parameters.Set(RenderTargetKeys.RenderTarget, engineContext.RenderContext.RenderTarget);
            effectMeshGroup.AddMesh(mainMesh);

        }
    }
}
