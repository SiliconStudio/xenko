// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Xaml.Markup;

namespace SiliconStudio.Xenko.Rendering
{
    [ContentProperty("Effect")]
    public class PostEffectMeshPlugin : PostEffectPlugin, IRenderPassPluginSource, IRenderPassPluginTarget
    {
        private EffectOld instantiatedEffect;
        private EffectMesh effectMesh;
        private Texture2D renderSource;

        public PostEffectMeshPlugin() : base(null)
        {
            ResizeFactor = 1.0f;
            PixelFormat = PixelFormat.R8G8B8A8_UNorm;
        }

        public float ResizeFactor { get; set; }

        public PixelFormat PixelFormat { get; set; }

        public EffectBuilder Effect { get; set; }

        public Texture2D RenderSource
        {
            get
            {
                if (renderSource == null)
                    renderSource = Texture.New2D(GraphicsDevice, (int)(RenderTarget.Width / ResizeFactor), (int)(RenderTarget.Height / ResizeFactor), PixelFormat, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                return renderSource;
            }
            set { renderSource = value; }
        }

        public RenderTarget RenderTarget { get; set; }

        public override void Load()
        {
            base.Load();

            Effect.Services = Services;
            instantiatedEffect = Effect.InstantiatePermutation();

            // Create mesh
            effectMesh = new EffectMesh(instantiatedEffect, null, "PostEffectMesh").KeepAliveBy(this);
            effectMesh.Parameters.Set(TexturingKeys.Texture0, RenderSource);
            effectMesh.Parameters.Set(RenderTargetKeys.RenderTarget, RenderTarget);

            effectMesh.EffectPass.RenderPass = RenderPass;

            // Register mesh for rendering
            RenderSystem.GlobalMeshes.AddMesh(effectMesh);
        }
    }
}
