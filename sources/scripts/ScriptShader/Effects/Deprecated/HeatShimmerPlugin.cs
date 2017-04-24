// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Light Shaft plugin.
    /// </summary>
    public class HeatShimmerPlugin : RenderPassPlugin, IRenderPassPluginSource, IRenderPassPluginTarget
    {
        private RenderPass boundingBoxPass, heatShimmerPass, heatShimmerComposePass;
        private List<EffectMesh> effectMeshes = new List<EffectMesh>();

        /// <summary>
        /// Initializes a new instance of class <see cref="LightShaftsPlugin"/>.
        /// </summary>
        public HeatShimmerPlugin() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of class <see cref="LightShaftsPlugin"/>.
        /// </summary>
        public HeatShimmerPlugin(string name)
            : base(name)
        {
            BoundingBoxes = new List<Mesh>();
            PreferredFormat = PixelFormat.R16G16B16A16_Float;
        }

        public ParameterCollection ViewParameters { get; set; }

        public PixelFormat PreferredFormat { get; set; }

        public Texture2D RenderSource
        {
            get;
            set;
        }

        public RenderTarget RenderTarget
        {
            get;
            set;
        }

        public DepthStencilBuffer DepthStencil { get; set; }

        public NoisePlugin NoisePlugin { get; set; }

        public List<Mesh> BoundingBoxes { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            boundingBoxPass = new RenderPass("BoundingBoxPass").KeepAliveBy(this);
            heatShimmerPass = new RenderPass("HeatShimmerPass").KeepAliveBy(this);
            heatShimmerComposePass = new RenderPass("HeatShimmerComposePass").KeepAliveBy(this);
        }

        public override void Load()
        {
            base.Load();

            RenderPass.AddPass(boundingBoxPass, heatShimmerPass, heatShimmerComposePass);

            // Use MinMax Plugin
            var bbRenderTargetPlugin = new RenderTargetsPlugin("BoundingBoxRenderTargetPlugin")
                {
                    EnableSetTargets = true,
                    EnableClearTarget = true,
                    RenderTarget = null,
                    RenderPass = boundingBoxPass,
                    Services = Services
                };
            bbRenderTargetPlugin.Apply();

            Parameters.AddSources(ViewParameters);
            Parameters.SetDefault(RenderTargetKeys.DepthStencilSource);
            Parameters.SetDefault(TexturingKeys.Sampler);
            bbRenderTargetPlugin.Parameters.AddSources(Parameters);

            EffectOld minMaxEffect = this.EffectSystemOld.BuildEffect("MinMax")
                .Using(new MinMaxShaderPlugin("MinMaxShaderPlugin") { RenderPassPlugin = bbRenderTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = bbRenderTargetPlugin })
                .KeepAliveBy(ActiveObjects)
                .InstantiatePermutation()
                .KeepAliveBy(ActiveObjects);

            heatShimmerPass.Parameters = new ParameterCollection();
            heatShimmerPass.Parameters.AddSources(Parameters);
            heatShimmerPass.Parameters.AddSources(NoisePlugin.Parameters);

            EffectOld heatShimmerEffect = this.EffectSystemOld.BuildEffect("HeatShimmer")
                .Using(new PostEffectSeparateShaderPlugin() { RenderPass = heatShimmerPass })
                .Using(
                    new BasicShaderPlugin(
                        new ShaderMixinSource()
                                        {
                                            Mixins = new List<ShaderClassSource>()
                                            {
                                            // TODO add support for IsZReverse
                                            //new ShaderClassSource("PostEffectHeatShimmer", Debug ? 1 : 0, effectSystemOld.IsZReverse ? 1 : 0, 3),
                                            new ShaderClassSource("PostEffectHeatShimmer", Debug ? 1 : 0, false ? 1 : 0, 3)},
                                            Compositions = new Dictionary<string, ShaderSource>() {
                                            {"NoiseSource", new ShaderClassSource("SimplexNoise")}},
                                            
                                        }
                        ) { RenderPass = heatShimmerPass })
                .KeepAliveBy(ActiveObjects)
                .InstantiatePermutation()
                .KeepAliveBy(ActiveObjects);

            EffectOld heatShimmerDisplayEffect = this.EffectSystemOld.BuildEffect("HeatShimmer")
                .Using(new PostEffectSeparateShaderPlugin() { RenderPass = heatShimmerComposePass })
                .Using(new BasicShaderPlugin(new ShaderClassSource("PostEffectHeatShimmerDisplay", Debug ? 1 : 0)) { RenderPass = heatShimmerComposePass })
                .KeepAliveBy(ActiveObjects)
                .InstantiatePermutation()
                .KeepAliveBy(ActiveObjects);

            if (OfflineCompilation)
                return;

            Parameters.Set(RenderTargetKeys.DepthStencilSource, DepthStencil.Texture);
            Parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.PointClamp);

            // ------------------------------------------
            // BoundingBox prepass
            // ------------------------------------------
            var renderTargetDesc = RenderSource.Description;
            var bbRenderTarget = Texture.New2D(GraphicsDevice, renderTargetDesc.Width, renderTargetDesc.Height, PixelFormat.R32G32_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget).KeepAliveBy(ActiveObjects);

            bbRenderTargetPlugin.RenderTarget = bbRenderTarget.ToRenderTarget();

            // Add meshes
            foreach (var bbMeshData in BoundingBoxes)
            {
                // Mesh for MinPass
                var bbMesh = new EffectMesh(minMaxEffect, bbMeshData).KeepAliveBy(ActiveObjects);
                // Add mesh
                // boundingBoxPass.AddPass(bbMesh.EffectMeshPasses[0].EffectPass);
                effectMeshes.Add(bbMesh);
                RenderSystem.GlobalMeshes.AddMesh(bbMesh);
            }

            // ------------------------------------------
            // Heat Compute
            // ------------------------------------------
            var shimmerTexture = Texture.New2D(GraphicsDevice, renderTargetDesc.Width, renderTargetDesc.Height, PixelFormat.R8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            var shimmerRenderTarget = shimmerTexture.ToRenderTarget();
            heatShimmerPass.StartPass += context => context.GraphicsDevice.Clear(shimmerRenderTarget, Color.Black);

            var quadMesh = new EffectMesh(heatShimmerEffect).KeepAliveBy(ActiveObjects);
            quadMesh.Parameters.Set(TexturingKeys.Texture1, bbRenderTarget);
            quadMesh.Parameters.Set(RenderTargetKeys.RenderTarget, shimmerRenderTarget);

            effectMeshes.Add(quadMesh);
            RenderSystem.GlobalMeshes.AddMesh(quadMesh);

            // ------------------------------------------
            // Heat display
            // ------------------------------------------
            quadMesh = new EffectMesh(heatShimmerDisplayEffect).KeepAliveBy(ActiveObjects);
            quadMesh.Parameters.Set(TexturingKeys.Texture0, RenderSource);
            quadMesh.Parameters.Set(TexturingKeys.Texture1, shimmerTexture);
            quadMesh.Parameters.Set(RenderTargetKeys.RenderTarget, RenderTarget);

            effectMeshes.Add(quadMesh);
            RenderSystem.GlobalMeshes.AddMesh(quadMesh);
        }

        public override void Unload()
        {
            if (!OfflineCompilation)
            {
                foreach (var effectMesh in effectMeshes)
                    RenderSystem.GlobalMeshes.RemoveMesh(effectMesh);
                effectMeshes.Clear();
            }

            RenderPass.RemovePass(boundingBoxPass);
            RenderPass.RemovePass(heatShimmerPass);
            RenderPass.RemovePass(heatShimmerComposePass);

            base.Unload();
        }
    }
}