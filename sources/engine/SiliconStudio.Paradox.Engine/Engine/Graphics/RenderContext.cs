// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Rendering context.
    /// </summary>
    public sealed class RenderContext
    {
        private readonly GraphicsDevice graphicsDevice;

        private readonly IServiceRegistry services;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderContext" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public RenderContext(IServiceRegistry services)
        {
            this.services = services;
            graphicsDevice = services.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphicsDevice;
            }
        }

        /// <summary>
        /// Gets or sets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceRegistry Services
        {
            get
            {
                return services;
            }
        }

        /// <summary>
        /// Gets or sets the parameters shared by this rendering context with shaders.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; set; }

        /// <summary>
        /// Allow to access tags setup on this render context
        /// </summary>
        public PropertyContainer Tags;
    }
}
