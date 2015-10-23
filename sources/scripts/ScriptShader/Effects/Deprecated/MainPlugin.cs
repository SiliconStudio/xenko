// Copyright (c) 2011 Silicon Studio

using System.Linq;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Plugin used for the main rendering view.
    /// </summary>
    public class MainPlugin : RenderPassPlugin
    {
        public static readonly PropertyKey<ParameterCollection> ViewParametersSourceKey = new PropertyKey<ParameterCollection>("ViewParametersSource", typeof(MainPlugin), new StaticDefaultValueMetadata(null)
            {
                PropertyUpdateCallback = delegate(ref PropertyContainer container, PropertyKey key, object newValue, object oldValue)
                    {
                        // ViewParameters is added to Parameters inheritance
                        if (oldValue != null)
                            ((RenderPassPlugin)container.Owner).Parameters.RemoveSource((ParameterCollection)oldValue);
                        if (newValue != null)
                            ((RenderPassPlugin)container.Owner).Parameters.AddSources((ParameterCollection)newValue);
                    }
            });

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPlugin"/> class.
        /// </summary>
        public MainPlugin() : this("Main")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MainPlugin(string name)
            : base(name)
        {
            ViewParameters = new RenderViewport("ViewParameters");
            ViewParameters.RegisterParameter(GlobalKeys.Time);
            ViewParameters.RegisterParameter(GlobalKeys.TimeStep);
            
            Parameters.AddSources(ViewParameters);
            //Parameters.RegisterParameter(TransformationKeys.ViewProjection);
        }

        public override void Initialize()
        {
            base.Initialize();

            bool isDepthStencilAsShaderResourceRequired = RenderSystem.ConfigContext.RenderPassPlugins.Any(plugin => plugin.Value.Tags.Get(RenderTargetKeys.RequireDepthStencilShaderResource));

            // By default, the MainPlugin is using the back buffer as the render target
            RenderTarget = GraphicsDevice.BackBuffer;

            // Create depth stencil
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
            var depthStencilFormat = PixelFormat.D16_UNorm;
#else
            var depthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
#endif

            if (GraphicsDevice.DepthStencilBuffer != null)
            {
                DepthStencil = GraphicsDevice.DepthStencilBuffer;
            }
            else
            {
                var depthStencil = Texture.New2D(GraphicsDevice, RenderTarget.Width, RenderTarget.Height, depthStencilFormat, TextureFlags.DepthStencil | (isDepthStencilAsShaderResourceRequired ? TextureFlags.ShaderResource : TextureFlags.None));
                DepthStencil = depthStencil.ToDepthStencilBuffer(false);
            }

            if (DepthStencilBuffer.IsReadOnlySupported(GraphicsDevice))
                DepthStencilReadOnly = DepthStencil.Texture.ToDepthStencilBuffer(true);
        }

        public RenderViewport ViewParameters { get; private set; }

        public RenderTarget RenderTarget { get; set; }

        public DepthStencilBuffer DepthStencil { get; set; }

        public DepthStencilBuffer DepthStencilReadOnly { get; set; }
    }
}
