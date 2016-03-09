// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using ComponentBase = SiliconStudio.Core.ComponentBase;
using IServiceRegistry = SiliconStudio.Core.IServiceRegistry;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Rendering context.
    /// </summary>
    public sealed class RenderContext : ComponentBase
    {
        private const string SharedImageEffectContextKey = "__SharedRenderContext__";
        private readonly GraphicsResourceAllocator allocator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderContext" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="allocator">The allocator.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        internal RenderContext(IServiceRegistry services, GraphicsResourceAllocator allocator = null)
        {
            if (services == null) throw new ArgumentNullException("services");
            Services = services;
            Effects = services.GetSafeServiceAs<EffectSystem>();
            this.allocator = allocator ?? new GraphicsResourceAllocator(Services).DisposeBy(this);
            GraphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
        }

        /// <summary>
        /// Occurs when a renderer is initialized.
        /// </summary>
        public event Action<IGraphicsRendererCore> RendererInitialized;

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        /// <value>The content manager.</value>
        public EffectSystem Effects { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <value>The time.</value>
        public GameTime Time { get; internal set; }

        /// <summary>
        /// Gets the <see cref="GraphicsResource"/> allocator.
        /// </summary>
        /// <value>The allocator.</value>
        public GraphicsResourceAllocator Allocator => allocator;

        /// <summary>
        /// Gets a global shared context.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>RenderContext.</returns>
        public static RenderContext GetShared(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");

            // Store RenderContext shared into the GraphicsDevice
            var graphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            return graphicsDevice.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerDevice, SharedImageEffectContextKey, d => new RenderContext(services));
        }

        internal void OnRendererInitialized(IGraphicsRendererCore obj)
        {
            Action<IGraphicsRendererCore> handler = RendererInitialized;
            if (handler != null) handler(obj);
        }
    }
}
