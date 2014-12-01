// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules
{
    public class GBufferRenderProcessor : RecursiveRenderer
    {
        private RenderTarget gbufferTextureRenderTarget;
        private Texture2D gbufferTexture;
        private Texture2D gbufferRenormTexture;
        private DepthStencilBuffer depthStencilBuffer;

        private bool useNormalPack;

        public GBufferRenderProcessor(IServiceRegistry services, RenderPipeline recursivePipeline, DepthStencilBuffer depthStencilBuffer, bool normalPack)
            : base(services, recursivePipeline)
        {
            this.depthStencilBuffer = depthStencilBuffer;
            useNormalPack = normalPack;
        }

        public Texture2D GBufferTexture
        {
            get { return gbufferTexture; }
        }

        protected override void OnRendering(RenderContext context)
        {
            // Setup render target
            GraphicsDevice.Clear(gbufferTextureRenderTarget, Color.Transparent);
            GraphicsDevice.Clear(depthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);
            GraphicsDevice.SetRenderTarget(depthStencilBuffer, gbufferTextureRenderTarget);

            base.OnRendering(context);
        }

        public override void Load()
        {
            base.Load();

            // Prepare GBuffer target
            gbufferTexture = Texture2D.New(GraphicsDevice, GraphicsDevice.BackBuffer.Width,
                GraphicsDevice.BackBuffer.Height, PixelFormat.R32G32B32A32_Float,
                TextureFlags.RenderTarget | TextureFlags.ShaderResource);

            gbufferTextureRenderTarget = gbufferTexture.ToRenderTarget();

            if (useNormalPack)
            {
                var assetManager = (AssetManager)Services.GetServiceAs<IAssetManager>();

                // Load normal packing data
                using (var texDataStream = assetManager.OpenAsStream("renorm.bin", StreamFlags.None))
                {
                    var texFileLength = texDataStream.Length;
                    var texData = new byte[texFileLength];
                    texDataStream.Read(texData, 0, (int)texFileLength);

                    gbufferRenormTexture = Texture2D.New(GraphicsDevice, 1024, 1024, PixelFormat.A8_UNorm, texData);
                    gbufferRenormTexture.Name = "Renorm";
                }

                RecursivePipeline.Parameters.Set(GBufferKeys.NormalPack, gbufferRenormTexture);
            }

            // Transmit main pass parameters for view
            RecursivePipeline.Parameters.AddSources(Pass.Parameters);

            Pass.Parameters.Set(GBufferBaseKeys.GBufferTexture, gbufferTexture);
        }

        public override void Unload()
        {
            base.Unload();

            // Remove parameters
            RecursivePipeline.Parameters.Remove(GBufferKeys.NormalPack);
            Pass.Parameters.Remove(GBufferBaseKeys.GBufferTexture);

            // Dispose GPU objects
            Utilities.Dispose(ref gbufferTextureRenderTarget);
            Utilities.Dispose(ref gbufferTexture);
            Utilities.Dispose(ref gbufferRenormTexture);
        }
    }
}