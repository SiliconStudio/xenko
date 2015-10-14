// Copyright (c) 2011 Silicon Studio

using System;
using System.IO;

using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Plugin used to render to a GBuffer from a MainPlugin.
    /// </summary>
    /// <remarks>
    /// This plugin depends on <see cref="MainPlugin"/> parameters.
    /// </remarks>
    public class GBufferPlugin : RenderTargetsPlugin, IRenderPassPluginTarget
    {
        private IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GBufferPlugin"/> class.
        /// </summary>
        public GBufferPlugin() : this("GBuffer")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GBufferPlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public GBufferPlugin(string name) :  base(name)
        {
            Parameters.SetDefault(GBufferBaseKeys.GBufferTexture);
            ClearColor = Color.Transparent;

            // Make sure that the Depth Stencil will be created with ShaderResource
            Tags.Set(RenderTargetKeys.RequireDepthStencilShaderResource, true);
        }

        /// <summary>
        /// Gets or sets the main plugin this instance is attached to.
        /// </summary>
        /// <value>
        /// The main plugin.
        /// </value>
        public MainPlugin MainPlugin { get; set; }

        public RenderTargetsPlugin MainTargetPlugin { get; set; }

        public Texture2D GBufferTexture
        {
            get { return Parameters.Get(GBufferBaseKeys.GBufferTexture); }
        }

        /// <summary>
        /// Gets or sets the depth stencil state Z read only.
        /// </summary>
        /// <value>
        /// The depth stencil state Z read only.
        /// </value>
        internal DepthStencilState DepthStencilStateZReadOnly { get; set; }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            graphicsDeviceService = Services.GetServiceAs<IGraphicsDeviceService>();
            var graphicsDevice = graphicsDeviceService.GraphicsDevice;


            Parameters.AddSources(MainPlugin.ViewParameters);

            if (OfflineCompilation)
                return;

            // Create texture used for normal packing
            var texture2D = Texture.New2D(GraphicsDevice, 1024, 1024, 1, PixelFormat.A8_UNorm);
            texture2D.Name = "Renorm";

            // Load normal packing data
            var texDataStream = VirtualFileSystem.OpenStream("/assets/effects/gbuffer/renorm.bin", VirtualFileMode.Open, VirtualFileAccess.Read);
            var texFileLength = texDataStream.Length;
            var texData = new byte[texFileLength];
            texDataStream.Read(texData, 0, (int)texFileLength);
            texture2D.SetData(graphicsDevice, texData);
            texDataStream.Dispose();

            // Force custom depth stencil state on main pass
            var mainDepthStencilState = MainTargetPlugin.Parameters.TryGet(EffectPlugin.DepthStencilStateKey) ?? graphicsDevice.DepthStencilStates.Default;
            MainTargetPlugin.Parameters.Set(EffectPlugin.DepthStencilStateKey, mainDepthStencilState);

            // Use depth stencil value from MainPlugin
            var defaultDescription = mainDepthStencilState.Description;
            ClearDepth = MainTargetPlugin.ClearDepth;

            // Use Default ZTest for GBuffer
            var depthStencilStateZStandard = DepthStencilState.New(GraphicsDevice, defaultDescription);
            depthStencilStateZStandard.Name = "ZStandard";

            Parameters.Set(EffectPlugin.DepthStencilStateKey, depthStencilStateZStandard);
            
            Parameters.Set(GBufferKeys.NormalPack, texture2D);
            Parameters.Set(TexturingKeys.PointSampler, graphicsDevice.SamplerStates.PointWrap);

            // MainPlugin is going to use the readonly depth stencil buffer
            if (DepthStencilBuffer.IsReadOnlySupported(GraphicsDevice))
            {
                MainTargetPlugin.UseDepthStencilReadOnly = true;
                MainTargetPlugin.Parameters.Set(RenderTargetKeys.DepthStencilSource, DepthStencil.Texture);
                MainTargetPlugin.DepthStencilReadOnly = DepthStencil.Texture.ToDepthStencilBuffer(true);
            }
            else
            {
                RenderPass.EndPass += (context) =>
                    {
                        //context.GraphicsDevice.Copy(DepthStencil
                        
                        //DepthStencil.SynchronizeReadonly(context.GraphicsDevice)
                    };
            }

            defaultDescription = mainDepthStencilState.Description;
            defaultDescription.DepthBufferWriteEnable = false;
            DepthStencilStateZReadOnly = DepthStencilState.New(GraphicsDevice,defaultDescription);
            DepthStencilStateZReadOnly.Name = "ZReadOnly";

            // Create normal texture (that LightPlugin will use)
            var gbufferTexture = Texture.New2D(GraphicsDevice, graphicsDevice.BackBuffer.Width, graphicsDevice.BackBuffer.Height, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            gbufferTexture.Name = "GBufferTexture";
            Parameters.Set(GBufferBaseKeys.GBufferTexture, gbufferTexture);
            RenderTarget = gbufferTexture.ToRenderTarget();

            // Set parameters for MainPlugin
            MainTargetPlugin.Parameters.Set(GBufferBaseKeys.GBufferTexture, gbufferTexture);
        }

        public override void ProcessModelView(RenderModelView2 effectModelView)
        {
            //var newEffectModelView = new EffectModelView();
            //foreach (var mesh in effectModelView.Meshes)
            //{
            //    var effect = Services.GetSafeServiceAs<IEffectSystemOld>().CreateEffect("GBuffer");
            //    var effectMesh = new EffectMesh(effect, mesh);
            //}
        }
    }
}
