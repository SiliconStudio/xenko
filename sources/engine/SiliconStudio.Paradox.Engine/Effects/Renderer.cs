// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Performs render pipeline transformations attached to a specific <see cref="RenderPass"/>.
    /// </summary>
    public abstract class Renderer
    {
        private readonly IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        protected Renderer(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");

            Services = services;
            RenderSystem = services.GetSafeServiceAs<RenderSystem>();
            EffectSystem = services.GetSafeServiceAs<EffectSystem>();
            graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return (graphicsDeviceService != null) ? graphicsDeviceService.GraphicsDevice : null;
            }
        }

        /// <summary>
        /// Gets the render system.
        /// </summary>
        /// <value>The render system.</value>
        public RenderSystem RenderSystem { get; private set; }

        /// <summary>
        /// Gets the effect system.
        /// </summary>
        /// <value>The effect system.</value>
        public EffectSystem EffectSystem { get; private set; }

        /// <summary>
        /// Gets the pass this processor is attached to.
        /// </summary>
        /// <value>The pass.</value>
        public RenderPass Pass { get; internal set; }

        /// <summary>
        /// Loads this instance. This method is called when a RenderPass is attached (directly or indirectly) to the children of <see cref="SiliconStudio.Paradox.Effects.RenderSystem.Pipeline"/>
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Unloads this instance. This method is called when a RenderPass is de-attached (directly or indirectly) to the children of <see cref="SiliconStudio.Paradox.Effects.RenderSystem.Pipeline"/>
        /// </summary>
        public abstract void Unload();
    }
}