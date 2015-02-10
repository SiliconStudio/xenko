// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Extension of the default <see cref="RendererBase"/> that initialize an EffectSystem and GraphicsDevice
    /// </summary>
    public abstract class RendererExtendedBase : RendererBase
    {
        private readonly IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RendererExtendedBase"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        protected RendererExtendedBase(IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException("services");
            Services = services;
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
        /// Gets the effect system.
        /// </summary>
        /// <value>The effect system.</value>
        public EffectSystem EffectSystem { get; private set; }

        protected CompilerParameters GetDefaultCompilerParameters()
        {
            var compilerParameters = new CompilerParameters();
            compilerParameters.Set(CompilerParameters.GraphicsProfileKey, GraphicsDevice.Features.Profile);
            return compilerParameters;
        }
    }
}