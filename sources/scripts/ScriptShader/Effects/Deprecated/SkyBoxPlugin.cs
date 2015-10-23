// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Posteffect manager.
    /// </summary>
    public class SkyBoxPlugin : RenderPassPlugin
    {
        public SkyBoxPlugin() : this(null)
        {
        }

        public SkyBoxPlugin(string name) : this(name, null)
        {
        }

        public SkyBoxPlugin(string name, ShaderSource skyBoxComposition)
            : base(name)
        {
            Texture = null;
            SkyBoxColor = skyBoxComposition;
        }

        public MainPlugin MainPlugin { get; set; }

        public RenderTargetsPlugin MainTargetPlugin { get; set; }

        internal EffectOld skyboxEffect;

        public Texture2D Texture
        {
            get
            {
                return Parameters.Get(TexturingKeys.Texture0);
            }
            set
            {
                Parameters.Set(TexturingKeys.Texture0, value);
            }
        }

        public ShaderSource SkyBoxColor { get; set; }

        public override void Load()
        {
            base.Load();

            skyboxEffect = this.EffectSystemOld.BuildEffect("Skybox")
                .Using(new StateShaderPlugin() { RenderPassPlugin = this, UseDepthStencilState = true })
                .Using(new BasicShaderPlugin(
                    new ShaderMixinSource() {
                        Mixins = new List<ShaderClassSource>() { new ShaderClassSource("SkyBox")},
                        Compositions = new Dictionary<string, ShaderSource>() { {"color", SkyBoxColor}}
                    }) { RenderPassPlugin = this })
                .InstantiatePermutation();

            if (OfflineCompilation)
                return;

            Parameters.AddSources(MainPlugin.ViewParameters);

            var zBackgroundValue = MainTargetPlugin.ClearDepth;
            // Generates a quad for post effect rendering (should be utility function)
            var vertices = new[]
                {
                    -1.0f, 1.0f, zBackgroundValue, 1.0f, 
                    1.0f, 1.0f, zBackgroundValue, 1.0f, 
                    -1.0f, -1.0f, zBackgroundValue, 1.0f,  
                    1.0f, -1.0f, zBackgroundValue, 1.0f, 
                };

            Parameters.RegisterParameter(EffectPlugin.DepthStencilStateKey);
            Parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearWrap);

            // Use the quad for this effectMesh
            var quadData = new Mesh();
            quadData.Draw = new MeshDraw
                {
                    DrawCount = 4,
                    PrimitiveType = PrimitiveType.TriangleStrip,
                    VertexBuffers = new[]
                                {
                                    new VertexBufferBinding(Buffer.Vertex.New(GraphicsDevice, vertices), new VertexDeclaration(VertexElement.Position<Vector4>()), 4)
                                }
                };

            RenderPass.StartPass += (context) =>
                {
                    // Setup the Viewport
                    context.GraphicsDevice.SetViewport(MainTargetPlugin.Viewport);

                    // Setup the depth stencil and main render target.
                    context.GraphicsDevice.SetRenderTarget(MainTargetPlugin.DepthStencil, MainTargetPlugin.RenderTarget);
                };

            RenderPass.EndPass += (context) => context.GraphicsDevice.UnsetRenderTargets();

            var skyboxMesh = new EffectMesh(skyboxEffect, quadData).KeepAliveBy(this);
            // If the main target plugin is not clearing anything, we assume that this is the job of the skybox plugin
            if (!MainTargetPlugin.EnableClearTarget && !MainTargetPlugin.EnableClearDepth)
            {
                var description = new DepthStencilStateDescription().Default();
                description.DepthBufferFunction = CompareFunction.Always;
                var alwaysWrite = DepthStencilState.New(GraphicsDevice, description);
                skyboxMesh.Parameters.Set(EffectPlugin.DepthStencilStateKey, alwaysWrite);
            }
            else
            {
                skyboxMesh.Parameters.Set(EffectPlugin.DepthStencilStateKey, MainTargetPlugin.DepthStencilState);
            }

            skyboxMesh.Parameters.AddSources(this.Parameters);
            RenderSystem.GlobalMeshes.AddMesh(skyboxMesh);
        }
    }
}