// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko;
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
    public class SlideShowPlugin : RenderPassPlugin, IRenderPassPluginTarget
    {
        private EffectOld slideShowEffect;

        public SlideShowPlugin()
            : this(null)
        {
        }

        public SlideShowPlugin(string name) : base(name)
        {
            Parameters.RegisterParameter(TexturingKeys.Texture0);
            Parameters.RegisterParameter(TexturingKeys.Texture2);
            Parameters.RegisterParameter(PostEffectTransitionKeys.ColorFactorFrom);
            Parameters.RegisterParameter(PostEffectTransitionKeys.ColorFactorTo);
            Parameters.RegisterParameter(PostEffectTransitionKeys.TransitionFactor);
            Parameters.RegisterParameter(PostEffectTransitionKeys.ZoomFactor);

            TransitionFactor = 0.0f;
            ZoomFactor = 1.0f;
            ColorFactorFrom = Color.White;
            ColorFactorTo = Color.White;
        }

        public MainPlugin MainPlugin { get; set; }

        public RenderTargetsPlugin MainTargetPlugin { get; set; }

        public Texture2D TextureFrom
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

        public Texture2D TextureTo
        {
            get
            {
                return Parameters.Get(TexturingKeys.Texture2);
            }
            set
            {
                Parameters.Set(TexturingKeys.Texture2, value);
            }
        }

        public Color ColorFactorFrom
        {
            get
            {
                return (Color)Parameters.Get(PostEffectTransitionKeys.ColorFactorFrom);
            }
            set
            {
                Parameters.Set(PostEffectTransitionKeys.ColorFactorFrom, value.ToVector4());
            }
        }

        public Color ColorFactorTo
        {
            get
            {
                return (Color)Parameters.Get(PostEffectTransitionKeys.ColorFactorTo);
            }
            set
            {
                Parameters.Set(PostEffectTransitionKeys.ColorFactorTo, value.ToVector4());
            }
        }

        public float TransitionFactor
        {
            get
            {
                return Parameters.Get(PostEffectTransitionKeys.TransitionFactor);
            }
            set
            {
                Parameters.Set(PostEffectTransitionKeys.TransitionFactor, value);
            }
        }

        public float ZoomFactor
        {
            get
            {
                return Parameters.Get(PostEffectTransitionKeys.ZoomFactor);
            }
            set
            {
                Parameters.Set(PostEffectTransitionKeys.ZoomFactor, value);
            }
        }

        public override void Load()
        {
            base.Load();

            slideShowEffect = this.EffectSystemOld.BuildEffect("SlideShow")
                .Using(new StateShaderPlugin() { RenderPassPlugin = this, UseDepthStencilState = true})
                .Using(new BasicShaderPlugin(new ShaderClassSource("PostEffectTransition")) { RenderPassPlugin = this })
                .InstantiatePermutation();

            if (OfflineCompilation)
                return;

            RenderPass.StartPass += (context) =>
                {
                    if (RenderPass.Enabled)
                    {
                        // Setup the Viewport
                        context.GraphicsDevice.SetViewport(MainTargetPlugin.Viewport);

                        // Setup the depth stencil and main render target.
                        context.GraphicsDevice.SetRenderTarget(RenderTarget);
                    }
                };

            RenderPass.EndPass += (context) =>
                {
                    if (RenderPass.Enabled)
                    {
                        context.GraphicsDevice.UnsetRenderTargets();
                    }
                };

            // Generates a quad for post effect rendering (should be utility function)
            var vertices = new[]
            {
                -1.0f,  1.0f, 
                 1.0f,  1.0f,
                -1.0f, -1.0f, 
                 1.0f, -1.0f,
            };

            // Use the quad for this effectMesh
            var quadData = new Mesh();
            quadData.Draw = new MeshDraw
            {
                DrawCount = 4,
                PrimitiveType = PrimitiveType.TriangleStrip,
                VertexBuffers = new[]
                            {
                                new VertexBufferBinding(Buffer.Vertex.New(GraphicsDevice, vertices), new VertexDeclaration(VertexElement.Position<Vector2>()), 4)
                            }
            };
            var textureMesh = new EffectMesh(slideShowEffect, quadData).KeepAliveBy(this);
            textureMesh.Parameters.Set(EffectPlugin.DepthStencilStateKey, GraphicsDevice.DepthStencilStates.None);

            textureMesh.Parameters.AddSources(this.Parameters);
            RenderSystem.GlobalMeshes.AddMesh(textureMesh);
        }

        public RenderTarget RenderTarget { get; set; }
    }
}