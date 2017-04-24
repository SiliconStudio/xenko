// Copyright (c) 2011 Silicon Studio

using System;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public enum AOQuality
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Posteffect manager.
    /// </summary>
    public class AOPlugin : RenderPassPlugin
    {
        public AOPlugin() : this(null)
        {
        }

        public AOPlugin(string name) : base(name)
        {
            Quality = AOQuality.Medium;
            UseNormal = true;
            BlurRadius = 7.0f;
            BlurSharpness = 1.0f;
            Radius = 35;
            Contrast = 1.25f;
            AngleBias = 30.0f;
            Attenuation = 1.0f;
            CountDirection = 16;
            CountStepMax = 8;
        }

        public GBufferPlugin GBufferPlugin { get; set; }

        public RenderTarget RenderTarget { get; set; }

        public bool UseNormal { get; set; }

        public AOQuality Quality { get; set; }

        public float BlurRadius { get; set; }

        public float BlurSharpness { get; set; }

        public float Radius { get; set; }

        public float Contrast { get; set; }

        public float AngleBias { get; set; }

        public float Attenuation { get; set; }

        public int CountDirection { get; set; }

        public int CountStepMax { get; set; }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Load()
        {
            base.Load();

            var postEffectsPlugin = new PostEffectGraphPlugin("PostEffectPlugin") { RenderPass = RenderPass };
            EffectOld hbaoEffect = this.EffectSystemOld.BuildEffect("HBAO")
                .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectsPlugin })
                .Using(new BasicShaderPlugin(new ShaderClassSource("PostEffectHBAO") { GenericArguments = new object[] { UseNormal ? 1 : 0, (int)Quality } }))
                .KeepAliveBy(ActiveObjects)
                .InstantiatePermutation()
                .KeepAliveBy(ActiveObjects);

            // Parameters.AddSources(MainPlugin.ViewParameters);
            
            if (OfflineCompilation)
                return;

            var colorTexture = (Texture2D)GBufferPlugin.MainTargetPlugin.RenderTarget.Texture;

            bool doBlur = true;
            bool halfResAO = false;

            //=================================================
            //Add hbao pass
            //==========================================
            //HBAO params
            int HBAO_numDir = 16; //TODO: should we recreate the random texture if we change this parameter?

            //==========================================
            //Create random texture
            var rng = new Random(0);
            Vector3[] tab = new Vector3[64 * 64];
            for (int i = 0; i < 64 * 64; i++)
            {
                float angle = (float)(2.0 * Math.PI * rng.NextDouble()) / (float)HBAO_numDir;
                Vector3 sample = new Vector3(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle),
                    (float)rng.NextDouble()
                    );
                tab[i] = sample;
            }

            var randomTexture = Texture.New2D(GraphicsDevice, 64, 64, PixelFormat.R32G32B32_Float, tab);

            var hbaoQuadMesh = new EffectMesh(hbaoEffect, name: "HBAO level").KeepAliveBy(ActiveObjects);

            //var renderTarget = renderingSetup.MainPlugin.RenderTarget;
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.RandomTexture, randomTexture);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.CountDirection, CountDirection);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.CountStepMax, CountStepMax);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.Radius, Radius);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.Attenuation, Attenuation);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.AngleBias, AngleBias * (float)Math.PI / 180.0f);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.Contrast, Contrast);
            hbaoQuadMesh.Parameters.Set(PostEffectHBAOKeys.RenderTargetResolutionRatio, halfResAO ? 0.5f : 1.0f);
            hbaoQuadMesh.Parameters.Set(GBufferBaseKeys.GBufferTexture, GBufferPlugin.GBufferTexture);
            hbaoQuadMesh.Parameters.Set(RenderTargetKeys.DepthStencilSource, GBufferPlugin.DepthStencil.Texture);

            if (!doBlur)
            {
                var aoRenderTarget = Texture.New2D(GraphicsDevice, GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, PixelFormat.R8_UNorm, TextureFlags.RenderTarget).ToRenderTarget().KeepAliveBy(ActiveObjects);
                aoRenderTarget.Name = "AOTexture"; 
                hbaoQuadMesh.Parameters.Set(RenderTargetKeys.RenderTarget, aoRenderTarget);
            }
            else
            {
                EffectOld hbaoBlurEffect = this.EffectSystemOld.BuildEffect("HBAO Blur")
                    .Using(new PostEffectShaderPlugin() { RenderPassPlugin = postEffectsPlugin })
                    .Using(new BasicShaderPlugin("PostEffectHBAOBlur"))
                    .InstantiatePermutation();

                //=====================================================================
                //BlurX
                EffectMesh hbaoBlurQuadMeshX = new EffectMesh(hbaoBlurEffect, name: "HBAO Blur X level");
                hbaoBlurQuadMeshX.Parameters.Set(PostEffectHBAOBlurKeys.BlurDirection, new Vector2(1, 0));
                hbaoBlurQuadMeshX.Parameters.Set(PostEffectHBAOBlurKeys.BlurRadius, BlurRadius);
                hbaoBlurQuadMeshX.Parameters.Set(PostEffectHBAOBlurKeys.BlurSharpness, BlurSharpness);
                hbaoBlurQuadMeshX.Parameters.Set(PostEffectHBAOBlurKeys.ColorTexture, colorTexture);
                hbaoBlurQuadMeshX.Parameters.Set(PostEffectHBAOBlurKeys.MultiplyResultWithColorTarget, false);
                hbaoBlurQuadMeshX.Parameters.Set(RenderTargetKeys.DepthStencilSource, GBufferPlugin.DepthStencil.Texture);

                //TODO: check the format (RGB 8 bits, or Float or half-float?)
                //TODO: check the resolution (can be half!), must update PostEffectSSDOKeys uniforms!
                var backBuffer = GraphicsDevice.BackBuffer;
                postEffectsPlugin.AddLink(
                    hbaoQuadMesh,
                    RenderTargetKeys.RenderTarget,
                    hbaoBlurQuadMeshX,
                    PostEffectHBAOBlurKeys.AmbiantOcclusionTexture,
                    new TextureDescription() { Width = backBuffer.Width >> (halfResAO ? 1 : 0), Height = backBuffer.Height >> (halfResAO ? 1 : 0), Format = PixelFormat.R8_UNorm });
                //hbaoBlurQuadMeshX.Parameters.Set(RenderTargetKeys.RenderTarget, engineContext.RenderContext.RenderTarget);

                //=====================================================================
                //BlurY
                EffectMesh hbaoBlurQuadMeshY = new EffectMesh(hbaoBlurEffect, name: "HBAO Blur Y level");
                hbaoBlurQuadMeshY.Parameters.Set(PostEffectHBAOBlurKeys.BlurDirection, new Vector2(0, 1));
                hbaoBlurQuadMeshY.Parameters.Set(PostEffectHBAOBlurKeys.BlurRadius, BlurRadius);
                hbaoBlurQuadMeshY.Parameters.Set(PostEffectHBAOBlurKeys.BlurSharpness, BlurSharpness);
                hbaoBlurQuadMeshY.Parameters.Set(PostEffectHBAOBlurKeys.ColorTexture, colorTexture);
                hbaoBlurQuadMeshY.Parameters.Set(PostEffectHBAOBlurKeys.MultiplyResultWithColorTarget, !Debug);
                hbaoBlurQuadMeshY.Parameters.Set(RenderTargetKeys.DepthStencilSource, GBufferPlugin.DepthStencil.Texture);

                //TODO: check the format (RGB 8 bits, or Float or half-float?)
                //TODO: check the resolution (can be half!), must update PostEffectSSDOKeys uniforms!
                postEffectsPlugin.AddLink(
                    hbaoBlurQuadMeshX,
                    RenderTargetKeys.RenderTarget,
                    hbaoBlurQuadMeshY,
                    PostEffectHBAOBlurKeys.AmbiantOcclusionTexture,
                    new TextureDescription { Width = backBuffer.Width, Height = backBuffer.Height, Format = PixelFormat.R8_UNorm });

                hbaoBlurQuadMeshY.Parameters.Set(RenderTargetKeys.RenderTarget, RenderTarget);
            }

            var effectMeshGroup = new RenderPassListEnumerator();
            foreach (var mesh in postEffectsPlugin.Meshes)
                RenderSystem.GlobalMeshes.AddMesh(mesh);

            // Link post effects (this will create intermediate surfaces)
            postEffectsPlugin.Resolve();
        }

        public override void Unload()
        {
            base.Unload();

            throw new NotImplementedException();
        }
    }
}